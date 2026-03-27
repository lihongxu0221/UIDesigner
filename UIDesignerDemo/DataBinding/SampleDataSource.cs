using System;
using System.Collections.Generic;
using System.ComponentModel;
using BgCommon.UI.Designer.DataBinding;

namespace UIDesignerDemo.DataBinding
{
    /// <summary>
    /// 示例数据源 - 演示数据绑定功能
    /// </summary>
    public class SampleDataSource : IDataSource, INotifyPropertyChanged
    {
        private string _name = "示例数据";
        private int _value = 100;
        private bool _isActive = true;
        private DateTime _timestamp = DateTime.Now;
        
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
        
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }
        
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    OnPropertyChanged(nameof(Timestamp));
                }
            }
        }
        
        public string DataSourceId => "SampleDataSource";
        
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                ["Name"] = Name,
                ["Value"] = Value,
                ["IsActive"] = IsActive,
                ["Timestamp"] = Timestamp
            };
        }
        
        public void SetData(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = value?.ToString() ?? string.Empty;
                    break;
                case "Value":
                    Value = Convert.ToInt32(value);
                    break;
                case "IsActive":
                    IsActive = Convert.ToBoolean(value);
                    break;
                case "Timestamp":
                    Timestamp = Convert.ToDateTime(value);
                    break;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}