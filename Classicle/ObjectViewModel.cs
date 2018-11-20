using System.ComponentModel;
using System.Runtime.CompilerServices;
using Classicle.Annotations;

namespace Classicle
{
    public class ObjectViewModel : INotifyPropertyChanged
    {
        private bool _isChecked;

        public string ObjectName { get; set; }
        public bool IsView { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
