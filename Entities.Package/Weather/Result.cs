using System.Collections.Generic;

namespace Entities.Weather
{
    /// <summary>
    ///     Multiple Results types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Result<T>
    {
        /// <summary>
        ///     Generic Result class constructor for list of results
        /// </summary>
        /// <param name="items">List of items to return</param>
        /// <param name="success">status of result</param>
        /// <param name="message">Possible error message</param>
        public Result(List<T> items, bool success, string message)
            : this()
        {
            Items = items;
            Success = success;
            Message = message;
        }

        /// <summary>
        ///     List of result items.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        ///     Operation result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Operation result status.
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    ///     Single Result type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct SingleResult<T>
    {
        /// <summary>
        ///     Generic Result class constructor for single result
        /// </summary>
        /// <param name="item">item to return</param>
        /// <param name="success">status of operation</param>
        /// <param name="message">possilbe error message</param>
        public SingleResult(T item, bool success, string message)
            : this()
        {
            Item = item;
            Success = success;
            Message = message;
        }

        /// <summary>
        ///     Result items.
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        ///     Operation result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Operation result status.
        /// </summary>
        public bool Success { get; set; }
    }
}