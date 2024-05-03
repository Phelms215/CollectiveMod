using System.Reflection;
using Collective.Definitions;
using HarmonyLib;
using UnityEngine;

namespace Collective.Utilities;

public class PatchUtility : MonoBehaviour
{
    private readonly Harmony _harmony = new(Key.NameSpace);
    private void Awake() => _harmony.PatchAll(Assembly.GetExecutingAssembly());
}