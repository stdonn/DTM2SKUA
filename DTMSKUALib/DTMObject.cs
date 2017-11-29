//
//  EmptyStruct.cs
//
//  Author:
//       Stephan Donndorf <stephan.donndorf@googlemail.com>
//
//  Copyright (c) 2016 Stephan Donndorf
//

using System.Collections.Generic;

namespace DTMSKUALib
{
	public struct DTMObject
	{
		public Dictionary<int, int[]> objects;
		public string configLine;
		public int objectNumber;
	}

	public struct StringObject
	{
		public float X {
			get; set;
		}

		public float Y {
			get; set;
		}

		public float Z {
			get; set;
		}

		public int ID {
			get; set;
		}
	}
}
