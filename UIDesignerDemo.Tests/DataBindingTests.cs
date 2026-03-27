using System;
using System.Collections.Generic;
using UIDesignerDemo.DataBinding;
using Xunit;

namespace UIDesignerDemo.Tests
{
    public class DataBindingTests
    {
        [Fact]
        public void SampleDataSource_ShouldImplementIDataSourceInterface()
        {
            // Arrange
            var dataSource = new SampleDataSource();
            
            // Act & Assert
            Assert.Equal("SampleDataSource", dataSource.DataSourceId);
            Assert.NotNull(dataSource.GetData());
        }
        
        [Fact]
        public void SampleDataSource_ShouldRaisePropertyChanged()
        {
            // Arrange
            var dataSource = new SampleDataSource();
            var propertyChangedRaised = false;
            dataSource.PropertyChanged += (sender, args) => propertyChangedRaised = true;
            
            // Act
            dataSource.Name = "新名称";
            
            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("新名称", dataSource.Name);
        }
        
        [Fact]
        public void SampleDataSource_GetData_ShouldReturnAllProperties()
        {
            // Arrange
            var dataSource = new SampleDataSource();
            
            // Act
            var data = dataSource.GetData();
            
            // Assert
            Assert.NotNull(data);
            Assert.Equal(4, data.Count);
            Assert.Contains("Name", data.Keys);
            Assert.Contains("Value", data.Keys);
            Assert.Contains("IsActive", data.Keys);
            Assert.Contains("Timestamp", data.Keys);
        }
        
        [Fact]
        public void SampleDataSource_SetData_ShouldUpdateProperties()
        {
            // Arrange
            var dataSource = new SampleDataSource();
            
            // Act
            dataSource.SetData("Name", "测试名称");
            dataSource.SetData("Value", 200);
            dataSource.SetData("IsActive", false);
            
            // Assert
            Assert.Equal("测试名称", dataSource.Name);
            Assert.Equal(200, dataSource.Value);
            Assert.False(dataSource.IsActive);
        }
    }
}