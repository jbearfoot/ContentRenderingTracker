using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace AlloyWithOutputCacheAttribute.Models.Blocks
{
    [ContentType(DisplayName = "NowBlock", GUID = "e029c8d9-de9f-42f2-9d36-0b2300208b1e", Description = "")]
    public class NowBlock : BlockData
    {
    }
}