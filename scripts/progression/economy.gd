extends RefCounted
class_name Economy

static func sell_coins(units: int, bonus: int = 0) -> int:
	return maxi(0, units) + maxi(0, bonus)

static func can_afford(balance: int, price: int) -> bool:
	return balance >= maxi(0, price)

static func buy(balance: int, price: int) -> int:
	if not can_afford(balance, price):
		return -1
	return balance - maxi(0, price)
