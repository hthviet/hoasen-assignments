import React, { useCallback, useState } from 'react';
import {
  View, Text, FlatList, StyleSheet,
  ActivityIndicator, RefreshControl, TouchableOpacity,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { ordersApi } from '../api';
import { useAuth } from '../context/AuthContext';
import { COLORS, formatPrice } from '../utils/constants';

function OrderCard({ item }) {
  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.orderId}>Order #{item.id}</Text>
        <Text style={styles.status}>{item.status}</Text>
      </View>
      <Text style={styles.date}>{new Date(item.orderDate).toLocaleString()}</Text>
      <Text style={styles.address}>Address: {item.shippingAddress}</Text>
      <Text style={styles.address}>Phone: {item.phoneNumber}</Text>
      <View style={styles.divider} />
      {item.items?.map((i, idx) => (
        <View key={`${item.id}-${idx}`} style={styles.itemRow}>
          <Text style={styles.itemName}>{i.productName} x{i.quantity}</Text>
          <Text style={styles.itemTotal}>{formatPrice(i.lineTotal)}</Text>
        </View>
      ))}
      <View style={styles.divider} />
      <View style={styles.itemRow}>
        <Text style={styles.totalLabel}>Total</Text>
        <Text style={styles.totalValue}>{formatPrice(item.totalAmount)}</Text>
      </View>
    </View>
  );
}

export default function OrdersScreen({ navigation }) {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchOrders = async (isRefresh = false) => {
    if (isRefresh) setRefreshing(true);
    else setLoading(true);
    try {
      const res = await ordersApi.getMyOrders();
      setOrders(res.data || []);
    } catch (_) {
      setOrders([]);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useFocusEffect(
    useCallback(() => {
      if (!token) {
        setOrders([]);
        setLoading(false);
        setRefreshing(false);
        return undefined;
      }
      fetchOrders();
      return undefined;
    }, [token])
  );

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  if (!token) {
    return (
      <View style={styles.centerPad}>
        <Text style={styles.empty}>Login required to view your orders.</Text>
        <TouchableOpacity style={styles.loginBtn} onPress={() => navigation.navigate('Auth')}>
          <Text style={styles.loginBtnText}>Go to Login</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <FlatList
        data={orders}
        keyExtractor={(item) => item.id.toString()}
        renderItem={({ item }) => <OrderCard item={item} />}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={() => fetchOrders(true)} />}
        ListEmptyComponent={<Text style={styles.empty}>No orders yet.</Text>}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  list: { padding: 12 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  centerPad: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, backgroundColor: COLORS.background },
  empty: { textAlign: 'center', marginTop: 40, color: COLORS.textSecondary, fontSize: 16 },
  loginBtn: { marginTop: 16, backgroundColor: COLORS.primary, borderRadius: 8, paddingHorizontal: 18, paddingVertical: 12 },
  loginBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 15 },
  card: { backgroundColor: COLORS.surface, borderRadius: 10, padding: 12, marginBottom: 12 },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  orderId: { fontSize: 16, fontWeight: 'bold', color: COLORS.text },
  status: { fontSize: 12, fontWeight: 'bold', color: COLORS.primary },
  date: { fontSize: 12, color: COLORS.textSecondary, marginTop: 4 },
  address: { fontSize: 13, color: COLORS.textSecondary, marginTop: 2 },
  divider: { height: 1, backgroundColor: COLORS.border, marginVertical: 8 },
  itemRow: { flexDirection: 'row', justifyContent: 'space-between', marginVertical: 2 },
  itemName: { flex: 1, color: COLORS.text, marginRight: 8 },
  itemTotal: { color: COLORS.text, fontWeight: '600' },
  totalLabel: { fontSize: 15, fontWeight: 'bold', color: COLORS.text },
  totalValue: { fontSize: 15, fontWeight: 'bold', color: COLORS.primary },
});
