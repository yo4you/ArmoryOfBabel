/// <summary>
/// interface used for enemies stun lock behaviour, enemies might define this diffrently eg. cancelling an attack routine when stunned
/// </summary>
internal interface IStunnable
{
	void Stun();

	void UnStun();
}