using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  InflatableElement is an ILayoutElement which allows making an object appear progressively, by 
	///  scaling it horizontally, vertically, or both. Contrarily to accessing the scale of the RectTransform,
	///  scaling with this script is considered by parent layout groups. 
	///  You can make the object appear and disappear with the methods Inflate() and Deflate(), 
	///  or access the scale parameters directly. 
	///  Caution, this model is currently kinda buggy as the visible scale is actually squared. 
	///  This is especially visible for scales > 1 and for fast transitions.                                </summary>
	///
	///=================================================================================================================
	public class InflatableElement : UIBehaviour, ILayoutElement
	{
		#region Declarations and simple properties

		[Header("Inflation behaviour")] //----------------------

		[SerializeField]
		[Tooltip("Is the object scaled horizontally when inflating?")]
		private bool _inflateHorizontally = true;

		[SerializeField]
		[Tooltip("Is the object scaled vertically when inflating?")]
		private bool _inflateVertically = true;

		[SerializeField]
		[Tooltip("How much time do deformations last?")]
		private float _transitionTime = 0.5f;

		[SerializeField]
		[Tooltip("Shall we deactivate the GameObject when fully deflated?")]
		private bool _deactivateWhenDeflated = true;

		[Header("Direct access to parameters")] //----------------------

        [SerializeField]
		[Tooltip("What does 'deflated' mean?")]
		private Vector2 _deflatedScale = Vector2.zero;

		[SerializeField]
		[Tooltip("What does 'inflated' mean?")]
		private Vector2 _inflatedScale = Vector2.one;

		[SerializeField]
		[Tooltip("How inflated is the object?")]
		[Range(0.0f, 1.0f)]
		private float _inflation = 1.0f;

		private int _priority = 10;
		private int _recursionSecurityCounter;

		private enum LayoutProperty { minWidth, minHeight, preferredWidth, preferredHeight, flexibleWidth, flexibleHeight };
		private enum InflationStatus { deflated, inflating, inflated, deflating };
		private InflationStatus _status;

		#endregion
		#region Initialization
		//=================================================================================================================
		//
		// I N I T I A L I Z A T I O N 
		//
		//=================================================================================================================


		///-------------------------------------------------------                                                  <summary>
		/// Use this for initialization                                                                             </summary>
		///-------------------------------------------------------
		new void Awake()
		{
			if (_inflation == 0.0f)
			{
				_status = InflationStatus.deflated;
			}
			else if (_inflation == 1.0f)
			{
				_status = InflationStatus.inflated;
			}
			else
			{
				_status = InflationStatus.inflating;
			}
			base.Awake();
		}


		#endregion
		#region Update
		//=================================================================================================================
		//
		// U P D A T E
		//
		//=================================================================================================================

		///-------------------------------------------------------                                                 <summary>
		/// Update is called once per frame                                                                            </summary>
		///-------------------------------------------------------
		void Update()
		{
			switch (_status)
			{
				case InflationStatus.inflating:
					_inflation = Mathf.Clamp01(_inflation + Time.deltaTime / _transitionTime);
					if (_inflation == 1.0f)
					{
						_status = InflationStatus.inflated;
					}
					SetDirty();
					break;

				case InflationStatus.deflating:
					_inflation = Mathf.Clamp01(_inflation - Time.deltaTime / _transitionTime);
					if (_inflation == 0.0f)
					{
						_status = InflationStatus.deflated;
						if (_deactivateWhenDeflated)
						{
							gameObject.SetActive(false);
						}
					}
					SetDirty();
					break;
			}
		}

		#endregion
		#region AUXILIARY METHODS AND PROPERTIES
		//=================================================================================================================
		//
		// A U X I L I A R Y   M E T H O D S   A N D    P R O P E R T I E S
		//
		//=================================================================================================================


		///-------------------------------------------------------                                                   <summary>                                             
		/// The scale for all width properties                                                                        </summary>
		///-------------------------------------------------------
		private float ScaleX
		{
			get
			{
				return _inflateHorizontally ? Mathf.Lerp(_deflatedScale.x, _inflatedScale.x, _inflation) : 1.0f;
			}
		}

		///-------------------------------------------------------                                                   <summary>                                             
		/// The scale for all height properties                                                                        </summary>
		///-------------------------------------------------------
		private float ScaleY
		{
			get
			{
				return _inflateVertically ? Mathf.Lerp(_deflatedScale.y, _inflatedScale.y, _inflation) : 1.0f;
			}
		}


		///-------------------------------------------------------                                                   <summary>                                             
		/// Used to get properties like minWidth from other Layout Elements                                         </summary>
		///-------------------------------------------------------
		private float ScaleProperty(LayoutProperty property)
		{
			// Prevent infinite recursion
			if (_recursionSecurityCounter++ > 20)
			{
				Debug.LogError("Infinite recursion for " + property);
				_recursionSecurityCounter = 0;
				return 0;
			}

			// Ignore this temporarry
			int oldPriority = _priority;
			_priority = -1;

			RectTransform rect = GetComponent<RectTransform>();
			float returnValue = 0.0f;

			switch (property)
			{
				case LayoutProperty.minWidth:
					returnValue = ScaleX * LayoutUtility.GetLayoutProperty(rect, e => e.minWidth, 0);
					break;

				case LayoutProperty.minHeight:
					returnValue = ScaleY * LayoutUtility.GetLayoutProperty(rect, e => e.minWidth, 0);
					break;

				case LayoutProperty.preferredWidth:
					returnValue = ScaleX * LayoutUtility.GetLayoutProperty(rect, e => e.preferredWidth, 0);
					break;

				case LayoutProperty.preferredHeight:
					returnValue = ScaleY * LayoutUtility.GetLayoutProperty(rect, e => e.preferredHeight, 0);
					break;

				case LayoutProperty.flexibleWidth:
					if (_recursionSecurityCounter <= 1)
					{
						returnValue = ScaleX * LayoutUtility.GetLayoutProperty(rect, e => e.flexibleWidth, 0);
					}
					break;

				case LayoutProperty.flexibleHeight:
					if (_recursionSecurityCounter <= 1)
					{
						returnValue = ScaleY * LayoutUtility.GetLayoutProperty(rect, e => e.flexibleHeight, 0);
					}
					break;
			}


			// Restoire this
			_priority = oldPriority;

			// Decrement recyrsion counter
			_recursionSecurityCounter = Mathf.Max(_recursionSecurityCounter - 1, 0);

			return returnValue;
		}

		///-------------------------------------------------------                                                   <summary>                                             
		/// Adapt local scale to scale parameters                                                                       </summary>
		///-------------------------------------------------------
		private void RecomputeLocalScale()
		{
			transform.localScale = new Vector3(ScaleX, ScaleY, 1.0f);
		}

		#endregion

		#region PUBLIC METHODS     
		//=================================================================================================================
		//
		// P U B L I C   M E T H O D S 
		//
		//=================================================================================================================

		///-------------------------------------------------------                                                   <summary>
		/// Start inflation transition                                                                      </summary>
		///-------------------------------------------------------
		public void Inflate()
		{
			gameObject.SetActive(true);
			if (_inflation < 1.0f)
			{
				_status = InflationStatus.inflating;
			}
		}

		///-------------------------------------------------------                                                   <summary>
		/// Start deflation transition                                                                      </summary>
		///-------------------------------------------------------
		public void Deflate()
		{
			if (_inflation > 0.0f)
			{
				_status = InflationStatus.deflating;
			}
		}

		#endregion

		#region OVERRIDDEN METHODS
		//=================================================================================================================
		//
		// O V E R R I D D E N   M E T H O D S 
		//
		//=================================================================================================================

		public virtual void CalculateLayoutInputHorizontal()
		{
		}

		public virtual void CalculateLayoutInputVertical()
		{
		}

		#endregion
		#region OVERRIDDEN PROPERTIES
		//=================================================================================================================
		//
		// O V E R R I D D E N   P R O P E R T I E S
		//
		//=================================================================================================================


		public virtual float minWidth { get { return ScaleProperty(LayoutProperty.minWidth); } }
		public virtual float minHeight { get { return ScaleProperty(LayoutProperty.minHeight); } }
		public virtual float preferredWidth { get { return ScaleProperty(LayoutProperty.preferredWidth); } }
		public virtual float preferredHeight { get { return ScaleProperty(LayoutProperty.preferredHeight); } }
		public virtual float flexibleWidth { get { return ScaleProperty(LayoutProperty.flexibleWidth); } }
		public virtual float flexibleHeight { get { return ScaleProperty(LayoutProperty.flexibleHeight); } }

		public virtual int layoutPriority
		{
			get
			{
				return _priority;
			}
		}

		#endregion
		#region Unity Lifetime calls

		//=================================================================================================================
		//
		// U N I T Y   L I F E T I M E   C A L L S
		//
		//=================================================================================================================

		protected override void OnEnable()
		{
			base.OnEnable();
			SetDirty();
		}

		protected override void OnTransformParentChanged()
		{
			SetDirty();
		}

		protected override void OnDisable()
		{
			SetDirty();
			base.OnDisable();
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetDirty();
		}

		protected override void OnBeforeTransformParentChanged()
		{
			SetDirty();
		}

		#endregion

		protected void SetDirty()
		{
			RecomputeLocalScale();

			if (!IsActive())
			{
				//  return;
			}

			LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirty();
		}

#endif
	}
}

