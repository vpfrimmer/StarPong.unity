using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                                              <summary>
    ///  CrossSectionSegment is an ordered pair of Vertex. CrossSectionSegment is for CrossSection what Triangle is for CustomMesh.	 </summary>
	///
	///=================================================================================================================
    public class CrossSectionSegment 
	{
		#region Declarations and simple properties

        public Vertex _A;
        public Vertex _B;

		#endregion
		#region Initialization
		//=================================================================================================================
		//
		// I N I T I A L I Z A T I O N 
		//
		//=================================================================================================================


		///-------------------------------------------------------                                                  <summary>
		/// Constructor                                                                           </summary>
		///-------------------------------------------------------
        public CrossSectionSegment(Vertex A, Vertex B)
        {
            _A = A;
            _B = B;
        }


		#endregion
	}
}

