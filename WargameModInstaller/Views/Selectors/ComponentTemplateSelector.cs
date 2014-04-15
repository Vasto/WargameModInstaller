using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WargameModInstaller.Model.Components;
using WargameModInstaller.ViewModels;

namespace WargameModInstaller.Views.Selectors
{
    public class ComponentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RequiredComponentTemplate
        {
            get;
            set;
        }

        public DataTemplate OptionalComponentTemplate
        {
            get;
            set;
        }

        public DataTemplate ExclusiveComponentTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var source = item as ComponentViewModel;
            if (source.Type == ComponentType.Required)
            {
                return RequiredComponentTemplate;
            }
            else if (source.Type == ComponentType.Optional)
            {
                return OptionalComponentTemplate;
            }
            else if (source.Type == ComponentType.Exclusive)
            {
                return ExclusiveComponentTemplate;
            }
            else
            {
                throw new InvalidOperationException("Unknow Component type. Cannot find matching template.");
            }
        }

    }
}
