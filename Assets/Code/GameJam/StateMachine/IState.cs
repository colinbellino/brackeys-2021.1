using Cysharp.Threading.Tasks;

namespace GameJam
{
	public interface IState
	{
		UniTask Enter();
		UniTask Exit();
		void Tick();
	}
}
