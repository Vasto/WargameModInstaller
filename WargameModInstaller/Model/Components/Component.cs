using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Components
{
    /// <summary>
    ///
    /// </summary>
    public class Component 
    {
        private bool isMarkedForInstall;
        private List<Component> childrenComponents;
        private Component parentComponent;

        public Component(ComponentType type, String name)
            : this(type, name, String.Empty)
        {
        }

        public Component(ComponentType type, String name, String text)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "The Type argument cannot be null");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name", "The Name argument cannot be null");
            }

            this.Type = type;
            this.Name = name;
            this.Text = text;
            this.childrenComponents = new List<Component>();
            this.isMarkedForInstall = Type == ComponentType.Required ? true : false;
        }

        public event EventHandler<EventArgs<bool>> IsMarkedForInstallChnaged;

        public ComponentType Type
        {
            get;
            private set;
        }

        public String Name
        {
            get;
            private set;
        }

        public String Text
        {
            get;
            private set;
        }

        public bool IsMarkedForInstall
        {
            get
            {
                return isMarkedForInstall;
            }
            set
            {
                if (isMarkedForInstall == value || Type == ComponentType.Required)
                {
                    return;
                }

                isMarkedForInstall = value;
                NotifyIsMarkedForInstallChanged(value);

                if (IsExclusiveMarkedForInstall())
                {
                    UnmarkSbilingExclusives();
                }

                if (!isMarkedForInstallChangePropagating)
                {
                    isMarkedForInstallChangePropagating = true;

                    Component source = this;
                    bool isStateChangeRequired = false;

                    PropagateMarkedForInstallChangeDown(source, value, isStateChangeRequired);
                    PropagateMarkedForInstallChangeUp(source, value, isStateChangeRequired);

                    isMarkedForInstallChangePropagating = false;
                }
            }
        }

        public IEnumerable<Component> Children
        {
            get
            {
                return childrenComponents;
            }
        }

        public Component Parent
        {
            get
            {
                return parentComponent;
            }
            set
            {
                parentComponent = value;
                foreach (var child in childrenComponents)
                {
                    child.parentComponent = value;
                }
            }
        }

        public void AddChild(Component child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child", "The child argument cannot be null");
            }

            if (!childrenComponents.Contains(child))
            {
                childrenComponents.Add(child);
                child.parentComponent = this;

                if (!isMarkedForInstall ||
                    child.IsOtherSiblingExclusiveMarkedForInstall())
                {
                    child.isMarkedForInstall = false;
                }
            }
        }

        public void RemoveChild(Component child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child", "The child argument cannot be null");
            }

            if (childrenComponents.Contains(child))
            {
                if (child.IsMarkedForInstall)
                {
                    child.MarkFirstSiblingExclusive();
                }

                childrenComponents.Remove(child);
                child.parentComponent = null;
            }
        }

        private void NotifyIsMarkedForInstallChanged(bool newValue)
        {
            var handler = IsMarkedForInstallChnaged;
            if (handler != null)
            {
                handler(this, new EventArgs<bool>(newValue));
            }
        }

        private static bool isMarkedForInstallChangePropagating = false;

        private void PropagateMarkedForInstallChangeUp(
            Component source,
            bool sourceNewValue,
            bool stateChangeRequired)
        {
            bool currentStateChanged = false;

            if (sourceNewValue == true)
            {
                if (!IsMarkedForInstall)
                {
                    IsMarkedForInstall = true;
                    currentStateChanged = true;
                }
            }
            else if (sourceNewValue == false)
            {
                bool noChildrenSelected = !Children.Any(c => c.IsMarkedForInstall);
                if (IsMarkedForInstall && noChildrenSelected)
                {
                    IsMarkedForInstall = false;
                    currentStateChanged = true;
                }
            }

            if (Parent != null && (!stateChangeRequired || currentStateChanged))
            {
                Parent.PropagateMarkedForInstallChangeUp(this, IsMarkedForInstall, true);
            }
        }

        private void PropagateMarkedForInstallChangeDown(
            Component source,
            bool sourceNewValue, 
            bool stateChangeRequired)
        {
            bool currentStateChanged = false;

            if (sourceNewValue == true)
            {
                if (!IsMarkedForInstall)
                {
                    IsMarkedForInstall = true;
                    currentStateChanged = true;
                }
            }
            else if (sourceNewValue == false)
            {
                if (IsMarkedForInstall)
                {
                    IsMarkedForInstall = false;
                    currentStateChanged = true;
                }
            }

            if (!stateChangeRequired || currentStateChanged)
            {
                var exclusiveChildren = Children
                    .Where(c => c.Type == ComponentType.Exclusive);
                if (sourceNewValue == true && exclusiveChildren.Count() > 0)
                {
                    exclusiveChildren.First()
                        .PropagateMarkedForInstallChangeDown(this, IsMarkedForInstall, true);
                }
                else
                {
                    foreach (var child in exclusiveChildren)
                    {
                        child.PropagateMarkedForInstallChangeDown(this, IsMarkedForInstall, true);
                    }
                }

                var otherChildren = Children
                    .Where(c => c.Type != ComponentType.Exclusive);
                foreach (var child in otherChildren)
                {
                    child.PropagateMarkedForInstallChangeDown(this, IsMarkedForInstall, true);
                }
            }
        }

        private bool IsExclusiveMarkedForInstall()
        {
            return IsMarkedForInstall && Type == ComponentType.Exclusive;
        }

        private bool IsOtherSiblingExclusiveMarkedForInstall()
        {
            if (Parent == null)
            {
                return false;
            }

            bool result = Parent.Children.Any(c => 
                    c != this &&
                    c.Type == ComponentType.Exclusive && 
                    c.IsMarkedForInstall);

            return result;
        }

        private void UnmarkSbilingExclusives()
        {
            if (Parent == null)
            {
                return;
            }

            var exclusiveSiblings = Parent.Children.Where(c => 
                c.Type == ComponentType.Exclusive
                && c != this);

            foreach (var sibling in exclusiveSiblings)
            {
                sibling.IsMarkedForInstall = false;
            }
        }

        private void MarkFirstSiblingExclusive()
        {
            if (Parent == null)
            {
                return;
            }

            var firstSibling = Parent.Children.FirstOrDefault(c => 
                    c.Type == ComponentType.Exclusive && 
                    c != this);

            firstSibling.IsMarkedForInstall = true;
        }

    }

}
