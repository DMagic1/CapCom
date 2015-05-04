using System;
using System.Collections.Generic;
using Contracts;
using Contracts.Agents;

using CapCom.Framework;

namespace CapCom
{
	public class CapComContract
	{
		private Guid id;
		private string name;
		private string briefing;
		private bool showNotes, canBeDeclined, canBeCancelled;
		private Contract root;
		private float totalFundsReward, totalRepReward, totalSciReward;
		private float totalFundsPenalty, totalRepPenalty;
		private string notes;
		private string fundsRew, fundsAdv, fundsPen, repRew, repPen, sciRew;
		private float fundsRewStrat, fundsAdvStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		private Agent agent;
		private List<CapComParameter> parameters = new List<CapComParameter>();
		private List<CapComParameter> allParameters = new List<CapComParameter>();

		public CapComContract(Contract c)
		{
			root = c;
			id = root.ContractGuid;
			name = root.Title;
			notes = root.Notes;
			briefing = root.Description;
			canBeDeclined = root.CanBeDeclined();
			canBeCancelled = root.CanBeCancelled();

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

			contractRewards();
			contractAdvance();
			contractPenalties();

			totalFundsReward = rewards();
			totalFundsPenalty = penalties();
			totalRepReward = repRewards();
			totalSciReward = sciRewards();
			totalRepPenalty = repPenalties();
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
	}
}
