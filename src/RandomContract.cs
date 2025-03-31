using System;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using AElf.Sdk.CSharp;
using AElf.Types;

namespace AElf.Contracts.RandomContract;

public class RandomContract : RandomContractContainer.RandomContractBase
{
    // Initializes the contract
    public override Empty Initialize(InitializeInput input)
    {
        // Check if the contract is already initialized
        Assert(State.Initialized.Value == false, "Already initialized.");
       
        // Check if the sender is the contract author
        State.GenesisContract.Value = Context.GetZeroSmartContractAddress();
        Assert(State.GenesisContract.GetContractAuthor.Call(Context.Self) == Context.Sender, "No permission.");
        
        // Set the contract state
        State.Admin.Value = Context.Sender;
        State.MaxValueLimit.Value = input.MaxValueLimit;
        State.MaxRandomNumberCount.Value = input.MaxRandomNumberCount;
        State.Initialized.Value = true;
        State.ConsensusContract.Value = Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);
        return new Empty();
    }

    public override Empty SetAdmin(Address input)
    {
        CheckAdminPermission();
        // Check if the input is valid
        Assert(input != null && !input.Value.IsNullOrEmpty(), "Invalid admin address.");
        // Set the new admin
        State.Admin.Value = input;
        return new Empty();
    }

    public override Empty SetMaxValueLimit(Int32Value input)
    {
        CheckAdminPermission();
        // Check if the input is valid
        Assert(input != null && input.Value > 0, "Invalid max value limit.");
        // Set the new max value limit
        State.MaxValueLimit.Value = input!.Value;
        return new Empty();
    }

    public override Empty SetMaxRandomNumberCount(Int32Value input)
    {
        CheckAdminPermission();
        // Check if the input is valid
        Assert(input != null && input.Value > 0, "Invalid max random number count.");
        // Set the new max random number count
        State.MaxRandomNumberCount.Value = input!.Value;
        return new Empty();
    }
    
    public override Empty GenerateRandomNumber(GenerateRandomNumberInput input)
    {
        // Check if the random number is already generated
        Assert(State.RandomNumbers[input.Hash] == null, "Random number already generated.");
        // Check if the maxValue is valid
        Assert(input.MaxValue > 0 && input.MaxValue <= State.MaxValueLimit.Value, "Invalid max value.");
        // Check if the random number count is valid
        Assert(
            input.RandomNumberCount > 0 && input.RandomNumberCount <= State.MaxRandomNumberCount.Value &&
            input.RandomNumberCount <= input.MaxValue, "Invalid random number count.");
        // Get a random hash and check if it is available
        var randomHash = State.ConsensusContract.GetRandomHash.Call(new Int64Value
        {
            Value = Context.CurrentHeight
        });
        Assert(randomHash != null && !randomHash.Value.IsNullOrEmpty(), "Still preparing your random number, please wait for a while...");

        var seedHash = HashHelper.ComputeFrom(input.Hash);
        randomHash = HashHelper.XorAndCompute(seedHash, randomHash);
            
        var randomNumbers = new List<int>();
        var retryTimes = 0;
        var maxRetryTimes = input.RandomNumberCount;
        for (var i = 0; i < input.RandomNumberCount; i++)
        {
            var randomInt = Math.Abs(int.Parse(randomHash.ToHex().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
            var randomNumber = randomInt % input.MaxValue;
            randomHash = HashHelper.XorAndCompute(seedHash, randomHash);
            if(randomNumbers.Contains(randomNumber))
            {
                retryTimes++;
                Assert(retryTimes < maxRetryTimes, "Failed to generate random numbers.");
                i--;
                continue;
            }
            randomNumbers.Add(randomNumber);
        }
            
        var randomNumberList = new RandomNumberList
        {
            Value = { randomNumbers }
        };
            
        State.RandomNumbers[input.Hash] = randomNumberList;
        Context.Fire(new RandomNumberGenerated
        {
            RandomNumbers = randomNumberList
        });
        return new Empty();
    }

    public override Address GetAdmin(Empty input)
    {
        return State.Admin.Value;
    }

    public override Int32Value GetMaxValueLimit(Empty input)
    {
        return new Int32Value
        {
            Value = State.MaxValueLimit.Value
        };
    }

    public override Int32Value GetMaxRandomNumberCount(Empty input)
    {
        return new Int32Value
        {
            Value = State.MaxRandomNumberCount.Value
        };
    }

    public override RandomNumberList GetRandomNumber(GetRandomNumberInput input)
    {
        return State.RandomNumbers[input.Hash];
    }
    
    private void CheckAdminPermission()
    {
        // Check if the sender is the admin
        Assert(Context.Sender == State.Admin.Value, "Only admin can perform this action.");
    }
}