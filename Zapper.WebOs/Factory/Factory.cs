﻿using System;

namespace Zapper.WebOs.Factory
{
    public class Factory<T> : IFactory<T>
    {
        private readonly Func<T> _creationFunction;

        public Factory(Func<T> creationFunction)
        {
            _creationFunction = creationFunction;
        }

        public T Create()
        {
            return _creationFunction();
        }
    }
}
