/*
 * Copyright 2007-2008 Hidekatsu Izuno
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */
using System;

namespace Wmf2SvgNet.Gdi.Wmf
{

    public class WmfParseException : Exception
	{
		/**
		 * The class fingerprint that is set to indicate serialization compatibility
		 * with a previous version of the class.
		 */
		private const long serialVersionUID = 42724981894237705L;

		public WmfParseException() : base() { }

		public WmfParseException(string message) : base(message) { }

		public WmfParseException(string message, Exception innerException) : base(message, innerException) { }

		public WmfParseException(Exception innerException) : base("One or more errors occured", innerException) { }
	}
}