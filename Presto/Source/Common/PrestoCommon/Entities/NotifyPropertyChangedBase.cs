﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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
