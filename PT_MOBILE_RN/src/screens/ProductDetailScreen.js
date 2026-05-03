import React, { useEffect, useState } from 'react';
import {
  View, Text, Image, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, Alert,
} from 'react-native';
import { productsApi } from '../api';
import { useCart } from '../context/CartContext';
import { COLORS, formatPrice } from '../utils/constants';

export default function ProductDetailScreen({ route, navigation }) {
  const { productId } = route.params;
  const { addItem } = useCart();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);

  useEffect(() => {
    productsApi.getById(productId)
      .then((res) => setProduct(res.data))
      .catch(() => Alert.alert('Error', 'Could not load product.'))
      .finally(() => setLoading(false));
  }, [productId]);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  if (!product) {
    return <View style={styles.center}><Text>Product not found.</Text></View>;
  }

  return (
    <ScrollView style={styles.container}>
      <Image
        source={{ uri: product.imageUrl || 'https://via.placeholder.com/400' }}
        style={styles.image}
        resizeMode="contain"
      />
      <View style={styles.content}>
        <Text style={styles.category}>{product.categoryName}</Text>
        <Text style={styles.brand}>{product.brand}</Text>
        <Text style={styles.name}>{product.name}</Text>
        <Text style={styles.price}>{formatPrice(product.price)}</Text>
        {/* <Text style={styles.stockLabel}>
          {product.stockQuantity > 0
            ? `In Stock (${product.stockQuantity} available)`
            : 'Out of Stock'}
        </Text> */}

        <View style={styles.qtyRow}>
          <TouchableOpacity style={styles.qtyBtn} onPress={() => setQuantity((q) => Math.max(1, q - 1))}>
            <Text style={styles.qtyBtnText}>-</Text>
          </TouchableOpacity>
          <Text style={styles.qtyValue}>{quantity}</Text>
          <TouchableOpacity
            style={styles.qtyBtn}
            onPress={() => setQuantity((q) => Math.min(product.stockQuantity, q + 1))}
          >
            <Text style={styles.qtyBtnText}>+</Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity
          style={[styles.addBtn, product.stockQuantity === 0 && styles.addBtnDisabled]}
          disabled={product.stockQuantity === 0}
          onPress={() => {
            addItem(product, quantity);
            Alert.alert('Added to Cart', `${quantity}x ${product.name}`);
          }}
        >
          <Text style={styles.addBtnText}>Add to Cart</Text>
        </TouchableOpacity>

        <Text style={styles.descTitle}>Description</Text>
        <Text style={styles.desc}>{product.description || 'No description available.'}</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  image: { width: '100%', height: 300, backgroundColor: '#f0f0f0' },
  content: { padding: 16 },
  category: { fontSize: 12, color: COLORS.textSecondary, textTransform: 'uppercase', marginBottom: 4 },
  brand: { fontSize: 14, color: COLORS.textSecondary, marginBottom: 4 },
  name: { fontSize: 20, fontWeight: 'bold', color: COLORS.text, marginBottom: 10 },
  price: { fontSize: 24, fontWeight: 'bold', color: COLORS.primary, marginBottom: 6 },
  stockLabel: { fontSize: 14, color: COLORS.success, marginBottom: 16 },
  qtyRow: { flexDirection: 'row', alignItems: 'center', marginBottom: 16 },
  qtyBtn: {
    backgroundColor: COLORS.border, borderRadius: 8,
    width: 36, height: 36, justifyContent: 'center', alignItems: 'center',
  },
  qtyBtnText: { fontSize: 20, fontWeight: 'bold', color: COLORS.text },
  qtyValue: { fontSize: 18, fontWeight: 'bold', marginHorizontal: 16, color: COLORS.text },
  addBtn: {
    backgroundColor: COLORS.primary, borderRadius: 10, padding: 16,
    alignItems: 'center', marginBottom: 24,
  },
  addBtnDisabled: { backgroundColor: COLORS.border },
  addBtnText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
  descTitle: { fontSize: 16, fontWeight: 'bold', color: COLORS.text, marginBottom: 8 },
  desc: { fontSize: 14, color: COLORS.textSecondary, lineHeight: 22 },
});
