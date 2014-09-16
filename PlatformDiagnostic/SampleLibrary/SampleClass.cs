using DD.Cloud.Aperture.DistributedManagement.Contracts;
using DD.Cloud.Aperture.Platform.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLibrary
{
	/// <summary>
	///		Class to demostrate using Roslyn to discover code assigning ServiceType.None to a variable.
	/// </summary>
	public class SampleClass
	{
		/// <summary>
		///		Sample class constructor.
		/// </summary>
		public SampleClass()
		{
		}

		public void SampleMethod()
		{
			ServiceType serviceTypeToProcess = ServiceType.None;
			int x;
			DoSomethingForService(serviceTypeToProcess);
			x = 1;
		}

		/// <summary>
		///		Do something to the specified service type.
		/// </summary>
		/// <param name="serviceType">
		///		The service type.
		/// </param>
		public void DoSomethingForService(ServiceType serviceType)
		{
			if (serviceType == ServiceType.None)
				throw new InvalidOperationException("Service type is none.");

			serviceType = ServiceType.None;
		}
	}
}
