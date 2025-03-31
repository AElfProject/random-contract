using AElf.Contracts.Consensus.AEDPoS;
using AElf.Standards.ACS0;

namespace AElf.Contracts.RandomContract;

public partial class RandomContractState
{
    internal ACS0Container.ACS0ReferenceState GenesisContract { get; set; }
    internal AEDPoSContractContainer.AEDPoSContractReferenceState ConsensusContract { get; set; }
}