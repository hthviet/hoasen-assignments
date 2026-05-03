import React from 'react';
import {
  View, Text, FlatList, TouchableOpacity, StyleSheet, Image, Alert,
} from 'react-native';
import { useCart } from '../context/CartContext';
import { COLORS, formatPrice } from '../utils/constants';

export default function CartScreen({ navigation }) {
  const { items, removeItem, updateQuantity, totalPrice, clearCart } = useCart();

  if (items.length === 0) {
    return (
      <View style={styles.empty}>
        <Text style={styles.emptyIcon}>🛒</Text>
        <Text style={styles.emptyText}>Your cart is empty</Text>
        <TouchableOpacity style={styles.shopBtn} onPress={() => navigation.navigate('Home')}>
          <Text style={styles.shopBtnText}>Start Shopping</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <FlatList
        data={items}
        keyExtractor={(item) => item.id.toString()}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Image
              source={{ uri: item.imageUrl || 'https://via.placeholder.com/80' }}
              style={styles.image}
              resizeMode="cover"
            />
            <View style={styles.info}>
              <Text style={styles.name} numberOfLines={2}>{item.name}</Text>
              <Text style={styles.price}>{formatPrice(item.price)}</Text>
              <View style={styles.qtyRow}>
                <TouchableOpacity style={styles.qtyBtn} onPress={() => updateQuantity(item.id, item.quantity - 1)}>
                  <Text style={styles.qtyBtnText}>-</Text>
                </TouchableOpacity>
                <Text style={styles.qty}>{item.quantity}</Text>
                <TouchableOpacity style={styles.qtyBtn} onPress={() => updateQuantity(item.id, item.quantity + 1)}>
                  <Text style={styles.qtyBtnText}>+</Text>
                </TouchableOpacity>
                <TouchableOpacity style={styles.removeBtn} onPress={() => removeItem(item.id)}>
                  <Text style={styles.removeBtnText}>Remove</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>
        )}
        contentContainerStyle={{ padding: 12 }}
      />

      <View style={styles.footer}>
        <View style={styles.totalRow}>
          <Text style={styles.totalLabel}>Total</Text>
          <Text style={styles.totalValue}>{formatPrice(totalPrice)}</Text>
        </View>
        <TouchableOpacity style={styles.clearBtn} onPress={() => Alert.alert('Clear Cart', 'Remove all items?', [
          { text: 'Cancel' },
          { text: 'Clear', style: 'destructive', onPress: clearCart },
        ])}>
          <Text style={styles.clearBtnText}>Clear Cart</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.checkoutBtn} onPress={() => navigation.navigate('Checkout')}>
          <Text style={styles.checkoutBtnText}>Proceed to Checkout</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  empty: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 32 },
  emptyIcon: { fontSize: 60, marginBottom: 16 },
  emptyText: { fontSize: 18, color: COLORS.textSecondary, marginBottom: 24 },
  shopBtn: { backgroundColor: COLORS.primary, borderRadius: 8, paddingHorizontal: 24, paddingVertical: 12 },
  shopBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
  card: {
    backgroundColor: COLORS.surface, borderRadius: 10, marginBottom: 10,
    flexDirection: 'row', overflow: 'hidden', elevation: 1,
  },
  image: { width: 90, height: 90 },
  info: { flex: 1, padding: 10, justifyContent: 'space-between' },
  name: { fontSize: 14, fontWeight: '600', color: COLORS.text },
  price: { fontSize: 14, fontWeight: 'bold', color: COLORS.primary },
  qtyRow: { flexDirection: 'row', alignItems: 'center', marginTop: 4 },
  qtyBtn: {
    backgroundColor: COLORS.border, borderRadius: 6,
    width: 28, height: 28, justifyContent: 'center', alignItems: 'center',
  },
  qtyBtnText: { fontSize: 16, fontWeight: 'bold' },
  qty: { fontSize: 16, fontWeight: 'bold', marginHorizontal: 10, color: COLORS.text },
  removeBtn: { marginLeft: 'auto', padding: 4 },
  removeBtnText: { color: COLORS.error, fontSize: 12 },
  footer: { backgroundColor: COLORS.surface, padding: 16, elevation: 8 },
  totalRow: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 12 },
  totalLabel: { fontSize: 18, fontWeight: 'bold', color: COLORS.text },
  totalValue: { fontSize: 18, fontWeight: 'bold', color: COLORS.primary },
  clearBtn: {
    borderWidth: 1, borderColor: COLORS.error, borderRadius: 8, padding: 12,
    alignItems: 'center', marginBottom: 8,
  },
  clearBtnText: { color: COLORS.error, fontWeight: 'bold' },
  checkoutBtn: { backgroundColor: COLORS.primary, borderRadius: 8, padding: 16, alignItems: 'center' },
  checkoutBtnText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
});
