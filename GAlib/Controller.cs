using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAlib
{
    public static class Controller
    {
        /// <summary>
        /// Converts a string to an enum element of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }
}
