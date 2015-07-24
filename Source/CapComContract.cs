#region license
/*The MIT License (MIT)
CapComContract - Object to store cached data about a contract

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
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Contracts;
using Contracts.Templates;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;
using FinePrint.Utilities;
using Contracts.Agents;
using UnityEngine;

using CapCom.Framework;

namespace CapCom
{
	public class CapComContract
	{
		private Guid id;
		private string name;
		private string briefing;
		private string target;
		private bool showNotes, canBeDeclined, canBeCancelled;
		private Contract root;
		private float totalFundsReward, totalRepReward, totalSciReward;
		private float totalFundsPenalty, totalRepPenalty;
		private double expire, deadline, completed;
		private string notes;
		private string fundsRew, fundsAdv, fundsPen, repRew, repPen, sciRew;
		private float fundsRewStrat, fundsAdvStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		private Agent agent;
		private List<CapComParameter> parameters = new List<CapComParameter>();
		private List<CapComParameter> allParameters = new List<CapComParameter>();

		public CapComContract(Contract c)
		{
			root = c;
			try
			{
				id = root.ContractGuid;
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Guid not set, skipping...: " + e);
				root = null;
				return;
			}

			try
			{
				name = root.Title;
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Title not set, using type name..: " + e);
				name = root.GetType().Name;
			}

			try
			{
				notes = root.Notes;
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Notes not set, blank notes used...: " + e);
				notes = "";
			}

			try
			{
				briefing = root.Description;
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Briefing not set, blank briefing used...: " + e);
				briefing = "";
			}

			try
			{
				canBeDeclined = root.CanBeDeclined();
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Decline state not set, using true...: " + e);
				canBeDeclined = true;
			}

			try
			{
				canBeCancelled = root.CanBeCancelled();
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Cancel state not set, using true...: " + e);
				canBeCancelled = true;
			}

			if (root.Agent != null)
				agent = root.Agent;
			else
				agent = AgentList.Instance.GetAgentRandom();

			for (int i = 0; i < root.ParameterCount; i++)
			{
				ContractParameter p = c.GetParameter(i);
				if (p == null)
					continue;

				addContractParam(p);
			}

			updateTimeValues();

			contractRewards();
			contractAdvance();
			contractPenalties();

			totalFundsReward = rewards();
			totalFundsPenalty = penalties();
			totalRepReward = repRewards();
			totalSciReward = sciRewards();
			totalRepPenalty = repPenalties();

			CelestialBody t = getTargetBody();

			target = t == null ? "" : t.name;
		}

		private void addContractParam(ContractParameter param)
		{
			CapComParameter cc = new CapComParameter(this, param, 0);
			parameters.Add(cc);
			allParameters.Add(cc);
		}

		private void contractRewards()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)root.FundsCompletion, root.ScienceCompletion, root.ReputationCompletion);

			fundsRew = "";
			if (root.FundsCompletion != 0)
				fundsRew = "+ " + root.FundsCompletion.ToString("N0");
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
			{
				fundsRew = string.Format("+ {0:N0} ({1:N0})", root.FundsCompletion + fundsRewStrat, fundsRewStrat);
			}

			repRew = "";
			if (root.ReputationCompletion != 0)
				repRew = "+ " + root.ReputationCompletion.ToString("N0");
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
			{
				repRew = string.Format("+ {0:N0} ({1:N0})", root.ReputationCompletion + repRewStrat, repRewStrat);
			}

			sciRew = "";
			if (root.ScienceCompletion != 0)
				sciRew = "+ " + root.ScienceCompletion.ToString("N0");
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRew = string.Format("+ {0:N0} ({1:N0})", root.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		private void contractAdvance()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractAdvance, (float)root.FundsAdvance, 0, 0);

			fundsAdv = "";
			if (root.FundsAdvance != 0)
				fundsAdv = "+ " + root.FundsAdvance.ToString("N0");
			fundsAdvStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsAdvStrat != 0f)
			{
				fundsAdv = string.Format("+ {0:N0} ({1:N0})", root.FundsAdvance + fundsAdvStrat, fundsAdvStrat);
			}
		}

		private void contractPenalties()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)root.FundsFailure, 0f, root.ReputationFailure);

			fundsPen = "";
			if (root.FundsFailure != 0)
				fundsPen = "- " + root.FundsFailure.ToString("N0");
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPen = string.Format("- {0:N0} ({1:N0})", root.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPen = "";
			if (root.ReputationFailure != 0)
				repPen = "- " + root.ReputationFailure.ToString("N0");
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPen = string.Format("- {0:N0} ({1:N0})", root.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		private float rewards()
		{
			float f = 0;
			f += (float)root.FundsCompletion + fundsRewStrat;
			f += (float)root.FundsAdvance + fundsAdvStrat;
			foreach (CapComParameter p in allParameters)
				f += (float)p.Param.FundsCompletion + p.FundsRewStrat;
			return f;
		}

		private float penalties()
		{
			float f = 0;
			f += (float)root.FundsFailure + fundsPenStrat;
			foreach (CapComParameter p in allParameters)
				f += (float)p.Param.FundsFailure + p.FundsPenStrat;
			return f;
		}

		private float repRewards()
		{
			float f = 0;
			f += root.ReputationCompletion + repRewStrat;
			foreach (CapComParameter p in allParameters)
				f += p.Param.ReputationCompletion + p.RepRewStrat;
			return f;
		}

		private float sciRewards()
		{
			float f = 0;
			f += root.ScienceCompletion + sciRewStrat;
			foreach (CapComParameter p in allParameters)
				f += p.Param.ScienceCompletion + p.SciRewStrat;
			return f;
		}

		private float repPenalties()
		{
			float f = 0;
			f += root.ReputationFailure + repPenStrat;
			foreach (CapComParameter p in allParameters)
				f += p.Param.ReputationFailure + p.RepPenStrat;
			return f;
		}

		private CelestialBody getTargetBody()
		{
			if (root == null)
				return null;

			bool checkTitle = false;

			Type t = root.GetType();

			if (t == typeof(CollectScience))
				return ((CollectScience)root).TargetBody;
			else if (t == typeof(ExploreBody))
				return ((ExploreBody)root).TargetBody;
			else if (t == typeof(PartTest))
			{
				var fields = typeof(PartTest).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[1].GetValue((PartTest)root) as CelestialBody;
			}
			else if (t == typeof(PlantFlag))
				return ((PlantFlag)root).TargetBody;
			else if (t == typeof(RecoverAsset))
			{
				var fields = typeof(RecoverAsset).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((RecoverAsset)root) as CelestialBody;
			}
			else if (t == typeof(GrandTour))
				return ((GrandTour)root).TargetBodies.LastOrDefault();
			else if (t == typeof(ARMContract))
			{
				var fields = typeof(ARMContract).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((ARMContract)root) as CelestialBody;
			}
			else if (t == typeof(BaseContract))
				return ((BaseContract)root).targetBody;
			else if (t == typeof(ISRUContract))
				return ((ISRUContract)root).targetBody;
			else if (t == typeof(RecordTrackContract))
				return null;
			else if (t == typeof(SatelliteContract))
			{
				SpecificOrbitParameter p = root.GetParameter<SpecificOrbitParameter>();

				if (p == null)
					return null;

				return p.targetBody;
			}
			else if (t == typeof(StationContract))
				return ((StationContract)root).targetBody;
			else if (t == typeof(SurveyContract))
				return ((SurveyContract)root).targetBody;
			else if (t == typeof(TourismContract))
				return null;
			else if (t == typeof(WorldFirstContract))
			{
				ProgressTrackingParameter p = root.GetParameter<ProgressTrackingParameter>();

				if (p == null)
					return null;

				var fields = typeof(ProgressTrackingParameter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				var milestone = fields[0].GetValue(p) as ProgressMilestone;

				if (milestone == null)
					return null;

				return milestone.body;
			}
			else
				checkTitle = true;

			if (checkTitle)
			{
				foreach (CelestialBody b in FlightGlobals.Bodies)
				{
					string n = b.name;

					Regex r = new Regex(string.Format(@"\b{0}\b", n));

					if (r.IsMatch(name))
						return b;
				}
			}

			return null;
		}

		public void addToParams(CapComParameter p)
		{
			if (!allParameters.Contains(p))
				allParameters.Add(p);
			else
				CC_MBE.LogFormatted("CapCom Parameter Object: [{0}] Already Present In Contract Container", p.Name);
		}

		public CapComParameter getParameter (int i)
		{
			if (parameters.Count >= i)
				return parameters[i];
			else
				CC_MBE.LogFormatted("CapCom Parameter List Index Out Of Range; Something Went Wrong Here...");

			return null;
		}

		public void updateTimeValues()
		{
			expire = root.DateExpire;
			if (expire <= 0)
				expire = double.MaxValue;

			deadline = root.DateDeadline;
			if (deadline <= 0)
				deadline = double.MaxValue;

			completed = root.DateFinished;
		}

		public Guid ID
		{
			get { return id; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Briefing
		{
			get { return briefing; }
		}

		public string Notes
		{
			get { return notes; }
		}

		public bool ShowNotes
		{
			get { return showNotes; }
			set { showNotes = value; }
		}

		public bool CanBeDeclined
		{
			get { return canBeDeclined; }
		}

		public bool CanBeCancelled
		{
			get { return canBeCancelled; }
		}

		public Contract Root
		{
			get { return root; }
		}

		public Agent RootAgent
		{
			get { return agent; }
		}

		public int ParameterCount
		{
			get { return parameters.Count; }
		}

		public float TotalReward
		{
			get { return totalFundsReward; }
		}

		public float TotalPenalty
		{
			get { return totalFundsPenalty; }
		}

		public float TotalRepReward
		{
			get { return totalRepReward; }
		}

		public float TotalRepPenalty
		{
			get { return totalRepPenalty; }
		}

		public float TotalSciReward
		{
			get { return totalSciReward; }
		}

		public string FundsRew
		{
			get { return fundsRew; }
		}

		public string FundsAdv
		{
			get { return fundsAdv; }
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

		public string Target
		{
			get { return target; }
		}

		public double Expire
		{
			get { return expire; }
		}

		public double Deadline
		{
			get { return deadline; }
		}

		public double Finished
		{
			get { return completed; }
		}
	}
}
