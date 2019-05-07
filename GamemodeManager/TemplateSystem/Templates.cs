using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GamemodeManager.TemplateSystem
{
	/// <inheritdoc />
	/// <summary>
	/// A collection of each template, indexed by PluginID
	/// </summary>
	public class Templates : IDictionary<string, Template>
	{
		/// <summary>
		/// Backing field for the <see cref="Count"/> property
		/// </summary>
		private int count;

		/// <summary>
		/// Initialize a new empty instance of <see cref="Templates"/>
		/// </summary>
		public Templates()
		{
			this.Keys = new List<string>();
			this.Values = new List<Template>();
		}

		/// <summary>
		/// Initialize a new empty instance of <see cref="Templates"/> of fixed size
		/// </summary>
		/// <param name="size">The size of the new instance</param>
		public Templates(int size)
		{
			this.Keys = new List<string>();
			this.Values = new List<Template>();
			this.IsFixedSize = true;
			this.Count = size;
		}


		/// <inheritdoc />
		/// <summary>
		/// The collection of keys in <see cref="T:GamemodeManager.TemplateSystem.Templates" />
		/// </summary>
		public ICollection<string> Keys { get; private set; }

		/// <inheritdoc />
		/// <summary>
		/// The collection of values in <see cref="Templates"/>
		/// </summary>
		public ICollection<Template> Values { get; private set; }

		/// <inheritdoc />
		/// <summary>
		/// Whether or not this collection can be modified
		/// </summary>
		public bool IsReadOnly =>
			(typeof(Templates).GetField("ReadOnlyField", BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentNullException()).IsInitOnly;

		/// <summary>
		/// Whether or not this collection is initialized with a fixed size
		/// </summary>
		public bool IsFixedSize { get; }

		/// <inheritdoc />
		/// <summary>
		/// The size of this collection
		/// </summary>
		public int Count
		{
			get => this.count = this.Keys.Count;
			set => this.count = value;
		}

		/// <inheritdoc />
		/// <summary>
		/// Return the <see cref="T:GamemodeManager.Templates.Template" /> at index specified by <see cref="!:key" />
		/// </summary>
		/// <param name="key">Where to access the value</param>
		/// <returns></returns>
		public Template this[string key]
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <inheritdoc />
		/// <summary>
		/// Whether the key is in the collection
		/// </summary>
		/// <param name="key">The key to be checked</param>
		/// <returns>The status of success</returns>
		public bool ContainsKey(string key) => 
			this.Keys.Contains(key);

		/// <inheritdoc />
		/// <summary>
		/// Add the following key value pair to the collection
		/// </summary>
		/// <param name="key">The key to add</param>
		/// <param name="value">The value to add</param>
		public void Add(string key, Template value)
		{
			this.Keys = new List<string>(this.Keys) { key };
			this.Values = new List<Template>(this.Values) { value };
		}

		/// <inheritdoc />
		/// <summary>
		/// Remove the specified key value pair
		/// </summary>
		/// <param name="item">The key value pair to remove</param>
		/// <returns>Whether the operation is successful</returns>
		public bool Remove(KeyValuePair<string, Template> item) =>
			this.Contains(item) && this.Keys.Remove(item.Key) && this.Values.Remove(item.Value);

		/// <inheritdoc />
		/// <summary>
		/// Remove the pair at the specified key
		/// </summary>
		/// <param name="key">The key at which to perform the remove action</param>
		/// <returns>Whether the operation is successful</returns>
		public bool Remove(string key) =>
			this.TryGetValue(key, out Template v) && this.Remove(new KeyValuePair<string, Template>(key, v));

		/// <inheritdoc />
		/// <summary>
		/// Safely return a value from this collection
		/// </summary>
		/// <param name="key">The key to search for the value of</param>
		/// <param name="value">The value to be returned on success</param>
		/// <returns>Whether the operation is successful</returns>
		public bool TryGetValue(string key, out Template value)
		{
			if (!this.ContainsKey(key))
			{
				value = null;
				return false;
			}

			value = this[key];
			return true;
		}
		
		/// <inheritdoc />
		/// <summary>
		/// Add the item to this collection
		/// </summary>
		/// <param name="item">The item to be added</param>
		public void Add(KeyValuePair<string, Template> item)
		{
			this.Keys.Add(item.Key);
			this.Values.Add(item.Value);
		}

		/// <inheritdoc />
		/// <summary>
		/// Empty the collection of all keys and values
		/// </summary>
		public void Clear()
		{
			this.Keys = new List<string>();
			this.Values = new List<Template>();
		}

		/// <inheritdoc />
		/// <summary>
		/// Determine if the <see cref="T:System.Collections.Generic.KeyValuePair`2" /> is present in the collection
		/// </summary>
		/// <param name="item">The item to be checked</param>
		/// <returns>Whether the operation is successful</returns>
		public bool Contains(KeyValuePair<string, Template> item) =>
			this.Keys.Contains(item.Key) && this.Values.Contains(item.Value);

		/// <inheritdoc />
		/// <summary>
		/// Copy this collection to a new array of <see cref="T:System.Collections.Generic.KeyValuePair`2" />
		/// </summary>
		/// <param name="array">The array to be copied to</param>
		/// <param name="arrayIndex">The index at which to start the copy</param>
		public void CopyTo(KeyValuePair<string, Template>[] array, int arrayIndex)
		{
			if (arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			int index = 0;
			while (arrayIndex++ < array.Length)
			{
				array[arrayIndex] = new KeyValuePair<string, Template>(this.Keys.ElementAt(index), this.Values.ElementAt(index));
				index++;
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Return the <see cref="T:System.Collections.IEnumerator" /> of this collection
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<string, Template>> GetEnumerator()
		{
			foreach (KeyValuePair<string, Template> x in this)
			{
				yield return x;
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Get the enumerator of this collection
		/// </summary>
		/// <returns><see cref="T:System.Collections.IEnumerator" /></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
