import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity,
  StyleSheet, Alert, ActivityIndicator, ScrollView,
} from 'react-native';
import { ordersApi } from '../api';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { COLORS, formatPrice } from '../utils/constants';

export default function CheckoutScreen({ navigation }) {
  const { token } = useAuth();
  const { items, totalPrice, clearCart } = useCart();
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [loading, setLoading] = useState(false);

  const handleCheckout = async () => {
    if (!token) {
      Alert.alert('Login Required', 'Please sign in before placing an order.', [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Login', onPress: () => navigation.navigate('Auth') },
      ]);
      return;
    }
    if (!phone.trim()) {
      Alert.alert('Validation', 'Please enter phone number.');
      return;
    }
    if (!address.trim()) {
      Alert.alert('Validation', 'Please enter shipping address.');
      return;
    }
    if (items.length === 0) {
      Alert.alert('Validation', 'Cart is empty.');
      return;
    }

    setLoading(true);
    try {
      const payloadItems = items.map((i) => ({ productId: i.id, quantity: i.quantity }));
      const res = await ordersApi.checkout(address.trim(), phone.trim(), payloadItems);
      const orderId = res.data?.orderId;
      clearCart();
      Alert.alert('Success', `Order #${orderId} placed successfully.`, [
        { text: 'View Orders', onPress: () => navigation.navigate('HomeTab', { screen: 'Orders' }) },
      ]);
    } catch (err) {
      const msg = err.response?.data?.message || 'Checkout failed. Please try again.';
      Alert.alert('Checkout Failed', msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <Text style={styles.heading}>Checkout</Text>

      {!token ? (
        <View style={styles.loginCard}>
          <Text style={styles.loginTitle}>Login required to checkout</Text>
          <Text style={styles.loginText}>Guests can browse products and add items to cart, but you need an account to place an order.</Text>
          <TouchableOpacity style={styles.loginBtn} onPress={() => navigation.navigate('Auth')}>
            <Text style={styles.loginBtnText}>Go to Login</Text>
          </TouchableOpacity>
        </View>
      ) : null}

      <View style={styles.card}>
        {items.map((item) => (
          <View key={item.id} style={styles.itemRow}>
            <Text style={styles.itemName} numberOfLines={1}>{item.name} x{item.quantity}</Text>
            <Text style={styles.itemPrice}>{formatPrice(item.price * item.quantity)}</Text>
          </View>
        ))}
        <View style={styles.separator} />
        <View style={styles.itemRow}>
          <Text style={styles.totalLabel}>Total</Text>
          <Text style={styles.totalPrice}>{formatPrice(totalPrice)}</Text>
        </View>
      </View>

      <TextInput
        style={styles.input}
        placeholder="Phone Number"
        placeholderTextColor={COLORS.textSecondary}
        keyboardType="phone-pad"
        value={phone}
        onChangeText={setPhone}
      />

      <TextInput
        style={[styles.input, styles.multiline]}
        placeholder="Shipping Address"
        placeholderTextColor={COLORS.textSecondary}
        multiline
        numberOfLines={3}
        value={address}
        onChangeText={setAddress}
      />

      <TouchableOpacity style={styles.checkoutBtn} onPress={handleCheckout} disabled={loading}>
        {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.checkoutBtnText}>Place Order</Text>}
      </TouchableOpacity>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  content: { padding: 16 },
  heading: { fontSize: 24, fontWeight: 'bold', color: COLORS.text, marginBottom: 12 },
  loginCard: { backgroundColor: COLORS.surface, borderRadius: 10, padding: 14, marginBottom: 16 },
  loginTitle: { fontSize: 16, fontWeight: 'bold', color: COLORS.text, marginBottom: 6 },
  loginText: { fontSize: 14, color: COLORS.textSecondary, lineHeight: 20, marginBottom: 12 },
  loginBtn: { backgroundColor: COLORS.primaryDark, borderRadius: 8, padding: 12, alignItems: 'center' },
  loginBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 15 },
  card: { backgroundColor: COLORS.surface, borderRadius: 10, padding: 12, marginBottom: 16 },
  itemRow: { flexDirection: 'row', justifyContent: 'space-between', marginVertical: 4 },
  itemName: { flex: 1, color: COLORS.text, fontSize: 14, marginRight: 8 },
  itemPrice: { color: COLORS.text, fontWeight: '600' },
  separator: { height: 1, backgroundColor: COLORS.border, marginVertical: 8 },
  totalLabel: { fontSize: 16, fontWeight: 'bold', color: COLORS.text },
  totalPrice: { fontSize: 16, fontWeight: 'bold', color: COLORS.primary },
  input: {
    backgroundColor: COLORS.surface, borderWidth: 1, borderColor: COLORS.border,
    borderRadius: 8, padding: 12, fontSize: 15, color: COLORS.text, marginBottom: 12,
  },
  multiline: { minHeight: 90, textAlignVertical: 'top' },
  checkoutBtn: {
    backgroundColor: COLORS.primary, borderRadius: 8, padding: 14,
    alignItems: 'center', marginTop: 6,
  },
  checkoutBtnText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
});
