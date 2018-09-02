using Ovresko.Generix.Core.Modules.Core.Module; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {

        public ModelFieldType FieldType { get; set; }
        public string Options { get; set; }
        public bool DoTranslate { get; set; }

        public Type OptionsType { get; set; }

        public ColumnAttribute(ModelFieldType fieldType, string options,bool _DoTranslate = false)
        {
            FieldType = fieldType;
            if (_DoTranslate)
            {
                Options = _(options);
            }
        }

        public ColumnAttribute(ModelFieldType fieldType, string options)
        {
            FieldType = fieldType;
            Options = options;
        }

        public ColumnAttribute(ModelFieldType fieldType, Type _OptionsType)
        {
            FieldType = fieldType;
            OptionsType = _OptionsType;
            Options = OptionsType.Name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RefreshViewAttribute : Attribute
    {
       
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class IsRefAttribute : Attribute
    {
        public IsRefAttribute(string typeName, bool Ignore = false)
        {
            this.Ignore = Ignore;
            TypeName = _(typeName);
        }

        public string TypeName { get; set; }
        public bool Ignore { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExDisplayName : DisplayNameAttribute
    {
        public ExDisplayName(string typeName) : base(_(typeName))
        {
            TypeName = base.DisplayNameValue;
        }   

      

        public string TypeName { get; set; } 
    }
    
    //[AttributeUsage(AttributeTargets.Method)]
    //public class OnChangeAttribute : Attribute
    //{
    //    public OnChangeAttribute(string propertyToWatch)
    //    {
    //        PropertyToWatch = propertyToWatch;
    //    }

    //    public string PropertyToWatch { get; set; }
    //}

    [AttributeUsage(AttributeTargets.Property)]
    public class AfterMapMethodAttribute : Attribute
    {
        public AfterMapMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; set; }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class myTypeAttribute : Attribute
    {
        public myTypeAttribute(Type type)
        {
            this.type = type;
        }

        public Type type { get; set; }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class IsSourceAttribute : Attribute
    {
        public IsSourceAttribute(string source)
        {
            this.source = source;
        }

        public string source { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IsBoldAttribute : Attribute
    {
        public IsBoldAttribute(bool isBod = true)
        {
            IsBod = isBod;
        }

        public bool IsBod { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LongDescriptionAttribute : Attribute
    {
        public LongDescriptionAttribute(string text)
        {
            this.text = text;
        }

        public string text { get; set; }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ShowInTableAttribute : Attribute
    {
        public ShowInTableAttribute(bool isShow = true)
        {
            IsShow = isShow;
        }

        public bool IsShow { get; set; }
    }

    /// <summary>
    /// SET COLUMN POSITION 1 OR 2
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SetColumnAttribute : Attribute
    {
        public SetColumnAttribute(int column)
        {
            this.column = column;
        }

        public int column { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DontShowInDetailAttribute : Attribute
    {
        public DontShowInDetailAttribute()
        {
        }
        
    }
}
