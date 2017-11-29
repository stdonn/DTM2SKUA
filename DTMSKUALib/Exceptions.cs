//
//  Exceptions.cs
//
//  Author:
//       Stephan Donndorf <stephan.donndorf@googlemail.com>
//
//  Copyright (c) 2016 Stephan Donndorf
//

using System;
using System.Runtime.Serialization;

namespace DTMSKUALib
{
	[System.Serializable]
	public class KeyExistingException : Exception
	{
		public KeyExistingException() : base ()
		{
		}

		public KeyExistingException( string message) : base( message )
		{
		}

		public KeyExistingException ( string message, Exception inner ) : base( message, inner )
		{
		}

		protected KeyExistingException ( SerializationInfo info, StreamingContext context ) : base( info, context )
		{
		}
	}
}
