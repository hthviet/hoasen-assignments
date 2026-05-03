import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { useAuth } from '../context/AuthContext';
import { COLORS } from '../utils/constants';

export default function ProfileScreen({ navigation }) {
  const { user, token, signOut } = useAuth();

  if (!token) {
    return (
      <View style={styles.container}>
        <View style={styles.card}>
          <Text style={styles.title}>Guest</Text>
          <Text style={styles.guestText}>You can browse products without an account. Sign in when you are ready to checkout and manage orders.</Text>
          <TouchableOpacity style={styles.loginBtn} onPress={() => navigation.navigate('Auth')}>
            <Text style={styles.loginBtnText}>Go to Login</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.card}>
        <Text style={styles.title}>My Profile</Text>
        <Text style={styles.label}>Full Name</Text>
        <Text style={styles.value}>{user?.fullName || '-'}</Text>
        <Text style={styles.label}>Email</Text>
        <Text style={styles.value}>{user?.email || '-'}</Text>

        <TouchableOpacity style={styles.logoutBtn} onPress={signOut}>
          <Text style={styles.logoutText}>Logout</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background, padding: 16 },
  card: { backgroundColor: COLORS.surface, borderRadius: 12, padding: 16 },
  title: { fontSize: 24, fontWeight: 'bold', color: COLORS.text, marginBottom: 16 },
  guestText: { fontSize: 15, color: COLORS.textSecondary, lineHeight: 22 },
  loginBtn: {
    marginTop: 18, backgroundColor: COLORS.primary,
    borderRadius: 8, padding: 14, alignItems: 'center',
  },
  loginBtnText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
  label: { fontSize: 13, color: COLORS.textSecondary, marginTop: 10 },
  value: { fontSize: 16, color: COLORS.text, fontWeight: '600' },
  logoutBtn: {
    marginTop: 24, backgroundColor: COLORS.error,
    borderRadius: 8, padding: 14, alignItems: 'center',
  },
  logoutText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
});
