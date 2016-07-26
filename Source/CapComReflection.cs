using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contracts;
using CapCom.Framework;

namespace CapCom
{
    public static class CapComReflection
	{
		internal static void loadMethods()
		{
			ccLoaded = loadCCPendingMethod();
			ccLoaded = loadCCLimitsMethod();
			ccLoaded = loadCCAcceptMethod();
		}

		private static bool ccLoaded;

		public static bool CCLoaded
		{
			get { return ccLoaded; }
		}

		private const string CCType = "ContractConfigurator.ConfiguredContract";
		private const string CCTypeCC = "ContractConfigurator.ContractConfigurator";
		private const string PendingName = "CurrentContracts";
		private const string CCLimitsName = "ContractLimit";
		private const string CCAcceptName = "CanAccept";

		private delegate IEnumerable CCPendingContracts();
		private delegate int CCLimits(Contract.ContractPrestige p);
		private delegate bool CCAccept(Contract c);

		private static CCPendingContracts _CCPendingContracts;
		private static CCLimits _CCLimits;
		private static CCAccept _CCAccept;

		internal static List<Contract> pendingContracts()
		{
			IEnumerable generic = _CCPendingContracts();
			List<Contract> pendingContractList = new List<Contract>();

			foreach (Object obj in generic)
			{
				if (obj == null)
					continue;
				Contract c = obj as Contract;

				pendingContractList.Add(c);
			}

			return pendingContractList;
		}

		internal static int prestigeLimits(Contract.ContractPrestige p)
		{
			return _CCLimits(p);
		}

		internal static bool canAccept(Contract c)
		{
			return _CCAccept(c);
		}

		private static bool loadCCPendingMethod()
		{
			try
			{
				Type CConfigType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
						.SingleOrDefault(t => t.FullName == CCType);

				if (CConfigType == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Type [{0}] Not Found", CCType);
					return false;
				}

				PropertyInfo CCPending = CConfigType.GetProperty(PendingName);

				if (CCPending == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Property [{0}] Not Loaded", PendingName);
					return false;
				}

				_CCPendingContracts = (CCPendingContracts)Delegate.CreateDelegate(typeof(CCPendingContracts), CCPending.GetGetMethod());

				CC_MBE.LogFormatted("Contract Configurator Pending Contracts Method Assigned");

				return _CCPendingContracts != null;
			}
			catch (Exception e)
			{
				CC_MBE.LogFormatted("Error in loading Contract Configurator methods\n{0}", e);
				return false;
			}
		}

		private static bool loadCCLimitsMethod()
		{
			try
			{
				Type CConfigType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
						.SingleOrDefault(t => t.FullName == CCTypeCC);

				if (CConfigType == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Type [{0}] Not Found", CCTypeCC);
					return false;
				}

				MethodInfo CCLimitsMethod = CConfigType.GetMethod(CCLimitsName, new Type[] { typeof(Contract.ContractPrestige) });

				if (CCLimitsMethod == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Method [{0}] Not Loaded", CCLimitsName);
					return false;
				}

				_CCLimits = (CCLimits)Delegate.CreateDelegate(typeof(CCLimits), CCLimitsMethod);

				CC_MBE.LogFormatted("Contract Configurator Contract Limits Method Assigned");

				return _CCLimits != null;
			}
			catch (Exception e)
			{
				CC_MBE.LogFormatted("Error in loading Contract Configurator methods\n{0}", e);
				return false;
			}
		}

		private static bool loadCCAcceptMethod()
		{
			try
			{
				Type CConfigType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
						.SingleOrDefault(t => t.FullName == CCTypeCC);

				if (CConfigType == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Type [{0}] Not Found", CCTypeCC);
					return false;
				}

				MethodInfo CCAcceptMethod = CConfigType.GetMethod(CCAcceptName, new Type[] { typeof(Contract) });

				if (CCAcceptMethod == null)
				{
					CC_MBE.LogFormatted("Contract Configurator Method [{0}] Not Loaded", CCAcceptName);
					return false;
				}

				_CCAccept = (CCAccept)Delegate.CreateDelegate(typeof(CCAccept), CCAcceptMethod);

				CC_MBE.LogFormatted("Contract Configurator Contract Can Accept Method Assigned");

				return _CCAccept != null;
			}
			catch (Exception e)
			{
				CC_MBE.LogFormatted("Error in loading Contract Configurator methods\n{0}", e);
				return false;
			}
		}
	}
}
