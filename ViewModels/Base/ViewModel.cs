using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HeadhunterGetterClient.ViewModels.Base
{
    internal abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName=null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string propertyNane = null) {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyNane);
            return true;
        }
    }
}
