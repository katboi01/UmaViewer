using System;
using UnityEngine;

namespace RuntimeGizmos
{
	public struct Square
	{
		public Vector3 bottomLeft;
		public Vector3 bottomRight;
		public Vector3 topLeft;
		public Vector3 topRight;

		public Vector3 this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return this.bottomLeft;
					case 1:
						return this.topLeft;
					case 2:
						return this.topRight;
					case 3:
						return this.bottomRight;
					case 4:
						return this.bottomLeft; //so we wrap around back to start
					default:
						return Vector3.zero;
				}
			}
		}
	}
}
