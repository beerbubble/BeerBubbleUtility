using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace BeerBubbleUtility
{
	public interface ICachingService
	{
		string Name
		{
			get;
		}
		
		int Count
		{
			get;
		}
		
		Hashtable State
		{
			get;
		}
		
		object Get( string key );

		void Remove( string key );

		List<string> RemoveByPattern( string keyPattern, bool returnMatchKeys );

		void Clear();

		void Add( string key, object value );

		void Add( string key, object value, int minutes );
		
		void Add( string key, object value, string filepath );

		void Add( string key, object value, int minutes, OnCacheRemoved onCacheRemoved );

		void Add( string key, object value, string filepath, OnCacheRemoved onCacheRemoved );

		void MicroInsert( string key, object value, int seconds );

        void MicroInsert( string key, object value, int seconds, string filePath );
    }
}
