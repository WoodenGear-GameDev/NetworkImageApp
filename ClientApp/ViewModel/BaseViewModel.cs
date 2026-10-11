using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Controller.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}