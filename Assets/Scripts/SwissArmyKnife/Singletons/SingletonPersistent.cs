using UnityEngine;
using System.Collections;

namespace SwissArmyKnife
{
	///=================================================================================================================
	///
	/// <summary>
	/// Singleton that is not destroyed automatically when loading a new scene.
	/// </summary>
	///
	///=================================================================================================================
	public class SingletonPersistent<T> : MonoBehaviour where T : SingletonPersistent<T>
	{
		private static T    _instance;

		///-------------------------------------------------------
		/// Instance
		/// - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		/// <summary>
		/// Gets the Singleton instance.
		/// </summary>
		///-------------------------------------------------------
		public static T		Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType(typeof(T)) as T;
					if (_instance == null)
					{ return null; }
				}
				return _instance;
			}
		}

		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
			if (_instance == null)
			{
				_instance = this as T;
			}
			else if (_instance != this as T)
			{
				DestroyImmediate(gameObject);
				return;
			}
			_instance.AwakeSingleton();
		}

		///-------------------------------------------------------
		/// AwakeSingleton
		/// - - - - - - - - - - - - - - - - - - - - - - - - - - - 
		/// <summary>
		/// Awakes the singleton. Replaces Unity Awake method.
		/// </summary>
		///-------------------------------------------------------
		public virtual void AwakeSingleton() { }
	}
}