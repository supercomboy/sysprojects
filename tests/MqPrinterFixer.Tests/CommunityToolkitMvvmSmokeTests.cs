using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
namespace MqPrinterFixer.Tests;

public class CommunityToolkitMvvmSmokeTests
{
    [Fact]
    public void ObservableObject_PropertyChanged_FiresOnSet()
    {
        // Arrange
        var target = new SmokeObservable();
        var fired = new List<string?>();
        target.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        // Act
        target.Name = "MQ Printer Fixer";

        // Assert
        Assert.Single(fired);
        Assert.Equal(nameof(SmokeObservable.Name), fired[0]);
        Assert.Equal("MQ Printer Fixer", target.Name);
    }

    private sealed partial class SmokeObservable : ObservableObject
    {
        private string? _name;

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}