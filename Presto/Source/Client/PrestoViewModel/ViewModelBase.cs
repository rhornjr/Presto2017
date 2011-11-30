using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PrestoViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (this.PropertyChanged == null) { return; }

            string propertyName = GetPropertyName(expression);

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;

            return memberExpression.Member.Name;
        }
    }
}
