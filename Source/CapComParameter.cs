#region license
/*The MIT License (MIT)
CapComParameter - Object to store cached data about a contract parameter

Copyright (c) 2015 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using Contracts;
using CapCom.Framework;
using FinePrint;
using FinePrint.Contracts.Parameters;
using System.Reflection;

namespace CapCom
{
	public class CapComParameter
	{
		private string name;
		private CapComContract root;
		private ContractParameter param;
		private string notes;
		private bool showNotes;
		private int level;
		private Waypoint waypoint;
		private string fundsRew, fundsPen, repRew, repPen, sciRew;
		private float fundsRewStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		private List<CapComParameter> parameters = new List<CapComParameter>();

		public CapComParameter(CapComContract r, ContractParameter p, int l)
		{
			root = r;
			param = p;
			name = param.Title;
			notes = param.Notes;
			level = l;

			if (level < 4)
			{
				for (int i = 0; i < param.ParameterCount; i++)
				{
					ContractParameter cp = param.GetParameter(i);
					if (cp == null)
						continue;

					addToParams(cp, level + 1);
				}
			}

			paramRewards();
			paramPenalties();

			checkForWaypoints();
		}

		private void checkForWaypoints()
		{
			waypoint = getWaypoint(param);
		}

		private Waypoint getWaypoint(ContractParameter cp)
		{
			Waypoint p = null;

			if (cp.GetType() == typeof(SurveyWaypointParameter))
			{
				SurveyWaypointParameter s = (SurveyWaypointParameter)param;
				if (s.State != ParameterState.Incomplete)
					return p;

				try
				{
					var field = (typeof(SurveyWaypointParameter)).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[0];
					p = (Waypoint)field.GetValue(s);
				}
				catch (Exception e)
				{
					CC_MBE.LogFormatted("Error While Assigning FinePrint Survey Waypoint Object... {0}", e);
				}
			}

			else if (cp.GetType() == typeof(StationaryPointParameter))
			{
				StationaryPointParameter s = (StationaryPointParameter)param;
				if (s.State != ParameterState.Incomplete)
					return p;

				try
				{
					var field = (typeof(StationaryPointParameter)).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[0];
					p = (Waypoint)field.GetValue(s);
				}
				catch (Exception e)
				{
					CC_MBE.LogFormatted("Error While Assigning FinePrint Stationary Waypoint Object... {0}", e);
				}
			}

			return p;
		}

		private void addToParams(ContractParameter p, int Level)
		{
			CapComParameter cc = new CapComParameter(root, p, Level);
			parameters.Add(cc);
			root.addToParams(cc);
		}

		private void paramRewards()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)param.FundsCompletion, param.ScienceCompletion, param.ReputationCompletion);
			fundsRew = "";
			if (param.FundsCompletion != 0)
				fundsRew = "+ " + param.FundsCompletion.ToString("N0");
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
			{
				fundsRew = string.Format("+ {0:N0} ({1:N0})", param.FundsCompletion + fundsRewStrat, fundsRewStrat);
			}

			repRew = "";
			if (param.ReputationCompletion != 0)
				repRew = "+ " + param.ReputationCompletion.ToString("N0");
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
			{
				repRew = string.Format("+ {0:N0} ({1:N0})", param.ReputationCompletion + repRewStrat, repRewStrat);
			}

			sciRew = "";
			if (param.ScienceCompletion != 0)
				sciRew = "+ " + param.ScienceCompletion.ToString("N0");
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRew = string.Format("+ {0:N0} ({1:N0})", param.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		private void paramPenalties()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)param.FundsFailure, 0f, param.ReputationFailure);

			fundsPen = "";
			if (param.FundsFailure != 0)
				fundsPen = "- " + param.FundsFailure.ToString("N0");
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPen = string.Format("- {0:N0} ({1:N0})", param.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPen = "";
			if (param.ReputationFailure != 0)
				repPen = "- " + param.ReputationFailure.ToString("N0");
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPen = string.Format("- {0:N0} ({1:N0})", param.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		public CapComParameter getParameter(int i)
		{
			if (parameters.Count >= i)
				return parameters[i];
			else
				CC_MBE.LogFormatted("CapCom Sub Parameter List Index Out Of Range; Something Went Wrong Here...");

			return null;
		}

		public string Name
		{
			get { return name; }
		}

		public string Notes
		{
			get { return notes; }
		}

		public int Level
		{
			get { return level; }
		}

		public Waypoint Way
		{
			get { return waypoint; }
		}

		public ContractParameter Param
		{
			get { return param; }
		}

		public bool ShowNotes
		{
			get { return showNotes; }
			set { showNotes = value; }
		}

		public int ParameterCount
		{
			get { return parameters.Count; }
		}

		public string FundsRew
		{
			get { return fundsRew; }
		}

		public string FundsPen
		{
			get { return fundsPen; }
		}

		public string RepRew
		{
			get { return repRew; }
		}

		public string RepPen
		{
			get { return repPen; }
		}

		public string SciRew
		{
			get { return sciRew; }
		}

		public float FundsRewStrat
		{
			get { return fundsRewStrat; }
		}

		public float FundsPenStrat
		{
			get { return fundsPenStrat; }
		}

		public float RepRewStrat
		{
			get { return repRewStrat; }
		}

		public float RepPenStrat
		{
			get { return repPenStrat; }
		}

		public float SciRewStrat
		{
			get { return sciRewStrat; }
		}
	}
}
