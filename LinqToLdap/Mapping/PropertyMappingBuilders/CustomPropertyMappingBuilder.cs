﻿using LinqToLdap.Helpers;
using LinqToLdap.Mapping.PropertyMappings;
using System;
using System.DirectoryServices.Protocols;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToLdap.Mapping.PropertyMappingBuilders
{
    internal class CustomPropertyMappingBuilder<T, TProperty> : IPropertyMappingBuilder, ICustomPropertyMapper<T, TProperty> where T : class
    {
        private Func<DirectoryAttribute, TProperty> _convertFrom;
        private Func<TProperty, object> _convertTo;
        private Func<TProperty, string> _convertToFilter;
        private Func<TProperty, TProperty, bool> _isEqual;

        public CustomPropertyMappingBuilder(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException("propertyInfo");
        }

        public bool IsDistinguishedName { get { return false; } }
        public ReadOnly? ReadOnlyConfiguration { get; private set; }

        public string AttributeName { get; private set; }
        public PropertyInfo PropertyInfo { get; set; }

        public string PropertyName
        {
            get { return PropertyInfo.Name; }
        }

        public IPropertyMapping ToPropertyMapping()
        {
            var arguments = new PropertyMappingArguments<T>
            {
                PropertyName = PropertyInfo.Name,
                PropertyType = PropertyInfo.PropertyType,
                AttributeName = AttributeName ?? PropertyInfo.Name.Replace('_', '-'),
                Getter = DelegateBuilder.BuildGetter<T>(PropertyInfo),
                Setter = DelegateBuilder.BuildSetter<T>(PropertyInfo),
                IsDistinguishedName = IsDistinguishedName,
                ReadOnly = IsDistinguishedName ? Mapping.ReadOnly.Always : ReadOnlyConfiguration.GetValueOrDefault(Mapping.ReadOnly.Never)
            };
            return new CustomPropertyMapping<T, TProperty>(arguments, _convertFrom, _convertTo, _convertToFilter, _isEqual);
        }

        public ICustomPropertyMapper<T, TProperty> Named(string attributeName)
        {
            AttributeName = attributeName.IsNullOrEmpty() ? null : attributeName;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertFromDirectoryUsing(Func<DirectoryAttribute, TProperty> converter)
        {
            _convertFrom = converter ?? throw new ArgumentNullException("converter");
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToFilterUsing(Func<TProperty, string> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            _convertToFilter = converter;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[][]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string[]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();

            return this;
        }

        public ICustomPropertyMapper<T, TProperty> CompareChangesUsing(Func<TProperty, TProperty, bool> comparer)
        {
            _isEqual = comparer ?? throw new ArgumentNullException("comparer");

            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ReadOnly(ReadOnly readOnly = Mapping.ReadOnly.Always)
        {
            ReadOnlyConfiguration = readOnly;

            return this;
        }
    }
}