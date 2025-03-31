using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.RandomContract;

// The state class is access the blockchain state
public partial class RandomContractState : ContractState 
{
    // A state to check if contract is initialized
    public BoolState Initialized { get; set; }
    public SingletonState<Address> Admin   { get; set; }
    public Int32State MaxValueLimit { get; set; }
    public Int32State MaxRandomNumberCount { get; set; }
    public MappedState<string, RandomNumberList> RandomNumbers { get; set; }
}