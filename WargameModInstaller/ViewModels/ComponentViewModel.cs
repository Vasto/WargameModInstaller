using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Model.Components;
using WargameModInstaller.Services.Utilities;

namespace WargameModInstaller.ViewModels
{
    //Na dobrą sprawę ten kod nie przewiduje żadnego prostego sposobu rozbudowy w przypadku dodania nowych typów componentów...

    public class ComponentViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMessageService messageService;

        private ComponentViewModel parent;
        private BindableCollection<ComponentViewModel> children;
        private bool isChecked;
        //private String text;

        public ComponentViewModel(
            IEventAggregator eventAggregator,
            IMessageService messageService, 
            Component component)
        {
            this.eventAggregator = eventAggregator;
            this.messageService = messageService;
            this.WrappedComponent = component;
            this.isChecked = component.IsMarkedForInstall;

            this.children = new BindableCollection<ComponentViewModel>();
            this.children.CollectionChanged += ChildrenCollectionChangedHandler;

            this.WrappedComponent.IsMarkedForInstallChnaged += ComponentSelectedForInstallChangedHandler;
        }

        public Component WrappedComponent
        {
            get;
            private set;
        }

        public ComponentViewModel Parent
        {
            get
            {
                return parent;
            }
            set
            {
                var oldValue = parent;
                if (oldValue == value)
                {
                    return;
                }

                parent = value;
                NotifyOfPropertyChange(() => Parent);

                UpdateParentChildRelation(oldValue, value);
            }
        }

        public BindableCollection<ComponentViewModel> Children
        {
            get
            {
                return children;
            }
            set
            {
                var oldVlaue = children;
                if (oldVlaue == value)
                {
                    return;
                }

                children = value;
                NotifyOfPropertyChange(() => Children);

                UpdateChildParentRelation(oldVlaue, value);

                if (oldVlaue != null)
                {
                    oldVlaue.CollectionChanged -= ChildrenCollectionChangedHandler;
                }

                if (children != null)
                {
                    children.CollectionChanged += ChildrenCollectionChangedHandler;
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked == value || Type == ComponentType.Required)
                {
                    return;
                }

                isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);

                WrappedComponent.IsMarkedForInstall = value;
            }
        }

        public ComponentType Type
        {
            get
            {
                return WrappedComponent.Type;
            }
        }

        public String Text
        {
            get
            {
                return WrappedComponent.Text;
            }
        }

        private void UpdateParentChildRelation(
            ComponentViewModel oldParent, 
            ComponentViewModel newParent)
        {
            if (newParent == null && oldParent.children.Contains(this))
            {
                oldParent.children.Remove(this);
            }
            else if (!parent.children.Contains(this))
            {
                parent.children.Add(this);
            }
        }

        private void UpdateChildParentRelation(
            IEnumerable<ComponentViewModel> oldChildren, 
            IEnumerable<ComponentViewModel> newChildren)
        {
            if (newChildren != null)
            {
                foreach (var child in newChildren)
                {
                    child.parent = this;
                }
            }

            if (oldChildren != null)
            {
                foreach (var oldChild in oldChildren)
                {
                    oldChild.parent = null;
                }
            }
        }

        private void ChildrenCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ComponentViewModel item in e.OldItems)
                {
                    item.parent = null;
                }
            }

            if (e.NewItems != null)
            {
                foreach (ComponentViewModel item in e.NewItems)
                {
                    item.parent = this;
                }
            }
        }

        private void ComponentSelectedForInstallChangedHandler(object sender, EventArgs<bool> e)
        {
            this.IsChecked = e.Value;
        }

    }
}
