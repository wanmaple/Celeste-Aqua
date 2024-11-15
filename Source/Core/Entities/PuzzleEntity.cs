using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Puzzle Entity")]
    [Tracked(true)]
    public class PuzzleEntity : Entity
    {
        private class PuzzleLightComparer : IComparer<PuzzleLight>
        {
            public int Compare(PuzzleLight lhs, PuzzleLight rhs)
            {
                return lhs.SequenceID.CompareTo(rhs.SequenceID);
            }
        }

        private static readonly PuzzleLightComparer SequenceComparer = new PuzzleLightComparer();

        public string PuzzleID { get; private set; }
        public Vector2 BonusPosition { get; protected set; }
        public SortedSet<PuzzleLight> RelatedLights { get; private set; } = new SortedSet<PuzzleLight>(SequenceComparer);

        public PuzzleEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            PuzzleID = data.Attr("puzzleId");
            BonusPosition = Position;
            string litOnMethod = data.Attr("litOnCheckMethod");
            string litOffMethod = data.Attr("litOffCheckMethod");
            string solveMethod = data.Attr("solveCheckMethod");
            string genMethod = data.Attr("generateBonusMethod");
            _litOnMethod = MatchMethod(litOnMethod, new ReflectionHelper.MethodDeclaration(typeof(bool), GetType(), typeof(PuzzleLight)));
            _litOffMethod = MatchMethod(litOffMethod, new ReflectionHelper.MethodDeclaration(typeof(bool), GetType(), typeof(PuzzleLight)));
            _solveMethod = MatchMethod(solveMethod, new ReflectionHelper.MethodDeclaration(typeof(bool), GetType()));
            _genMethod = MatchMethod(genMethod, new ReflectionHelper.MethodDeclaration(typeof(IEnumerator), GetType()));
            if (_litOnMethod == null && !string.IsNullOrEmpty(litOnMethod))
            {
                AquaDebugger.LogWarning("The LitOnCheckMethod parameter may be incorrect. Make sure the format is 'mod:ModName/ClassName/FunctionName' and the function must be static, boolean return and accept PuzzleEntity, PuzzleLight as parameters.");
            }
            if (_litOffMethod == null && !string.IsNullOrEmpty(litOffMethod))
            {
                AquaDebugger.LogWarning("The LitOffCheckMethod parameter may be incorrect. Make sure the format is 'mod:ModName/ClassName/FunctionName' and the function must be static, boolean return and accept PuzzleEntity, PuzzleLight as parameters.");
            }
            if (_solveMethod == null && !string.IsNullOrEmpty(solveMethod))
            {
                AquaDebugger.LogWarning("The SolveCheckMethod parameter may be incorrect. Make sure the format is 'mod:ModName/ClassName/FunctionName' and the function must be static, boolean return and accept PuzzleEntity as parameter.");
            }
            if (_genMethod == null && !string.IsNullOrEmpty(solveMethod))
            {
                AquaDebugger.LogWarning("The GenerateBonusMethod parameter may be incorrect. Make sure the format is 'mod:ModName/ClassName/FunctionName' and the function must be static, IEnumerator return and accept PuzzleEntity as parameter.");
            }
        }

        public override void Update()
        {
            if (!_puzzleClosed && IsSolved())
            {
                Add(new Coroutine(GenerateBonus()));
                _puzzleClosed = true;
            }
            base.Update();
        }

        public virtual bool CanLitOn(PuzzleLight light)
        {
            if (_puzzleClosed) 
                return false;

            if (_litOnMethod != null)
            {
                return (bool)_litOnMethod.Invoke(null, new object[] { this, light });
            }
            return false;
        }

        public virtual bool CanLitOff(PuzzleLight light)
        {
            if (_puzzleClosed)
                return false;

            if (_litOffMethod != null)
            {
                return (bool)_litOffMethod.Invoke(null, new object[] { this, light });
            }
            return false;
        }

        protected virtual bool IsSolved()
        {
            if (_solveMethod!=null)
            {
                return (bool)_solveMethod.Invoke(null, new object[] { this });
            }
            return false;
        }

        protected virtual IEnumerator GenerateBonus()
        {
            if (_genMethod != null)
            {
                yield return _genMethod.Invoke(null, new object[] { this }) as IEnumerator;
            }
            yield return null;
        }

        private MethodInfo MatchMethod(string text, ReflectionHelper.MethodDeclaration declaration)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (text.StartsWith("mod:"))
            {
                string identifier = text.Substring(4);
                string[] path = identifier.Split('.');
                if (path.Length >= 3)
                {
                    string modName = path[0];
                    string className = path[1];
                    string methodName = path[2];
                    return ReflectionHelper.FindMethodInMod(modName, className, methodName, declaration);
                }
            }
            return null;
        }

        private MethodInfo _litOnMethod;
        private MethodInfo _litOffMethod;
        private MethodInfo _solveMethod;
        private MethodInfo _genMethod;
        protected bool _puzzleClosed = false;
    }
}
