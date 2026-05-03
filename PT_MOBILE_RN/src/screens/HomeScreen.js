import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, FlatList, TextInput, TouchableOpacity,
  StyleSheet, Image, ActivityIndicator, ScrollView,
} from 'react-native';
import { productsApi } from '../api';
import { useCart } from '../context/CartContext';
import { COLORS, formatPrice } from '../utils/constants';

export default function HomeScreen({ navigation }) {
  const { addItem, totalItems } = useCart();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [sort, setSort] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchProducts = useCallback(async (p = 1, reset = false) => {
    if (loading) return;
    setLoading(true);
    try {
      const params = { page: p };
      if (search) params.search = search;
      if (selectedCategory) params.categoryId = selectedCategory;
      if (sort) params.sort = sort;
      const res = await productsApi.getAll(params);
      const { items, totalPages: tp } = res.data;
      setProducts((prev) => (reset ? items : [...prev, ...items]));
      setTotalPages(tp);
      setPage(p);
    } catch (_) {}
    setLoading(false);
  }, [search, selectedCategory, sort, loading]);

  useEffect(() => {
    productsApi.getCategories().then((res) => setCategories(res.data));
  }, []);

  useEffect(() => {
    setProducts([]);
    fetchProducts(1, true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, selectedCategory, sort]);

  const loadMore = () => {
    if (!loading && page < totalPages) {
      fetchProducts(page + 1);
    }
  };

  const renderProduct = ({ item }) => (
    <TouchableOpacity
      style={styles.card}
      onPress={() => navigation.navigate('ProductDetail', { productId: item.id })}
    >
      <Image
        source={{ uri: item.imageUrl || 'https://via.placeholder.com/150' }}
        style={styles.image}
        resizeMode="cover"
      />
      <View style={styles.cardBody}>
        <Text style={styles.brand}>{item.brand}</Text>
        <Text style={styles.name} numberOfLines={2}>{item.name}</Text>
        <Text style={styles.price}>{formatPrice(item.price)}</Text>
        <Text style={styles.stock}>Stock: {item.stockQuantity}</Text>
        <TouchableOpacity
          style={styles.addBtn}
          onPress={() => addItem(item)}
        >
          <Text style={styles.addBtnText}>Add to Cart</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );

  return (
    <View style={styles.container}>
      {/* Search Bar */}
      <View style={styles.searchRow}>
        <TextInput
          style={styles.searchInput}
          placeholder="Search products..."
          placeholderTextColor={COLORS.textSecondary}
          value={search}
          onChangeText={setSearch}
          returnKeyType="search"
        />
      </View>

      {/* Category Filter */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.catScroll}>
        <TouchableOpacity
          style={[styles.catChip, !selectedCategory && styles.catChipActive]}
          onPress={() => setSelectedCategory(null)}
        >
          <Text style={[styles.catChipText, !selectedCategory && styles.catChipTextActive]}>All</Text>
        </TouchableOpacity>
        {categories.map((c) => (
          <TouchableOpacity
            key={c.id}
            style={[styles.catChip, selectedCategory === c.id && styles.catChipActive]}
            onPress={() => setSelectedCategory(selectedCategory === c.id ? null : c.id)}
          >
            <Text style={[styles.catChipText, selectedCategory === c.id && styles.catChipTextActive]}>{c.name}</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Sort */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.sortScroll}>
        {[['', 'Default'], ['price_asc', 'Price Up'], ['price_desc', 'Price Down']].map(([val, label]) => (
          <TouchableOpacity
            key={val}
            style={[styles.sortChip, sort === val && styles.sortChipActive]}
            onPress={() => setSort(val)}
          >
            <Text style={[styles.sortChipText, sort === val && styles.sortChipTextActive]}>{label}</Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Product List */}
      <FlatList
        data={products}
        renderItem={renderProduct}
        keyExtractor={(item) => item.id.toString()}
        numColumns={2}
        columnWrapperStyle={styles.row}
        contentContainerStyle={styles.list}
        onEndReached={loadMore}
        onEndReachedThreshold={0.5}
        ListFooterComponent={loading ? <ActivityIndicator style={{ padding: 16 }} color={COLORS.primary} /> : null}
        ListEmptyComponent={!loading ? <Text style={styles.empty}>No products found.</Text> : null}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  searchRow: { padding: 12, paddingBottom: 4 },
  searchInput: {
    backgroundColor: COLORS.surface, borderWidth: 1, borderColor: COLORS.border,
    borderRadius: 8, padding: 10, fontSize: 16, color: COLORS.text,
  },
  catScroll: { paddingHorizontal: 8, paddingVertical: 6, flexGrow: 0 },
  catChip: {
    borderWidth: 1, borderColor: COLORS.border, borderRadius: 16,
    paddingHorizontal: 12, paddingVertical: 6, marginHorizontal: 4,
    backgroundColor: COLORS.surface,
  },
  catChipActive: { backgroundColor: COLORS.primary, borderColor: COLORS.primary },
  catChipText: { fontSize: 13, color: COLORS.text },
  catChipTextActive: { color: '#fff' },
  sortScroll: { paddingHorizontal: 8, paddingBottom: 6, flexGrow: 0 },
  sortChip: {
    borderWidth: 1, borderColor: COLORS.border, borderRadius: 16,
    paddingHorizontal: 12, paddingVertical: 5, marginHorizontal: 4,
    backgroundColor: COLORS.surface,
  },
  sortChipActive: { backgroundColor: COLORS.accent, borderColor: COLORS.accent },
  sortChipText: { fontSize: 13, color: COLORS.text },
  sortChipTextActive: { color: '#fff' },
  list: { padding: 8 },
  row: { justifyContent: 'space-between' },
  card: {
    backgroundColor: COLORS.surface, borderRadius: 10, margin: 4,
    flex: 0.5, overflow: 'hidden', elevation: 2,
    shadowColor: '#000', shadowOffset: { width: 0, height: 1 }, shadowOpacity: 0.1, shadowRadius: 2,
  },
  image: { width: '100%', height: 140, backgroundColor: '#eee' },
  cardBody: { padding: 10 },
  brand: { fontSize: 11, color: COLORS.textSecondary, textTransform: 'uppercase' },
  name: { fontSize: 13, fontWeight: '600', color: COLORS.text, marginTop: 2, marginBottom: 6 },
  price: { fontSize: 14, fontWeight: 'bold', color: COLORS.primary },
  stock: { fontSize: 11, color: COLORS.textSecondary, marginTop: 2 },
  addBtn: {
    backgroundColor: COLORS.primary, borderRadius: 6, padding: 7,
    alignItems: 'center', marginTop: 8,
  },
  addBtnText: { color: '#fff', fontSize: 12, fontWeight: 'bold' },
  empty: { textAlign: 'center', color: COLORS.textSecondary, marginTop: 32, fontSize: 16 },
});
