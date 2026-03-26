/**
 * Home/Dashboard Page
 * Premium SaaS Dashboard - Material 3 inspired design
 */

import { useEffect } from 'react';
import { useAuth } from '@/store/authStore';
import { Layout } from '@/components/Layout';

export const HomePage = () => {
  const { user, isAuthenticated, hydrate } = useAuth();

  useEffect(() => {
    hydrate();
  }, [hydrate]);

  return (
    <Layout>
      <div className="space-y-8">
        {/* Premium Welcome Hero Card */}
        <div className="relative group">
          <div className="absolute inset-0 bg-gradient-to-r from-primary-600 via-primary-500 to-purple-600 rounded-2xl blur-xl opacity-30 group-hover:opacity-40 transition duration-300"></div>
          <div className="relative bg-gradient-to-br from-primary-600 to-primary-700 rounded-2xl p-8 text-white border border-primary-500 border-opacity-20 shadow-xl overflow-hidden">
            {/* Background decoration */}
            <div className="absolute top-0 right-0 w-32 h-32 bg-white opacity-5 rounded-full -mr-16 -mt-16"></div>
            <div className="absolute bottom-0 left-0 w-24 h-24 bg-white opacity-5 rounded-full -ml-12 -mb-12"></div>
            
            <div className="relative">
              <h2 className="text-4xl font-bold mb-3">
                Bem-vindo de volta{user?.name ? `, ${user.name}!` : '!'}
              </h2>
              <p className="text-primary-100 text-lg leading-relaxed">
                Gerencie suas finanças com inteligência e simplicidade. Seu painel está pronto para começar.
              </p>
            </div>
          </div>
        </div>

        {/* Premium Dashboard Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Total de Gastos Card */}
          <div className="group relative bg-white rounded-2xl border border-slate-200 p-6 hover:border-primary-300 transition shadow-sm hover:shadow-xl hover:shadow-primary-500/10">
            <div className="absolute top-0 right-0 w-20 h-20 bg-primary-50 rounded-full -mr-8 -mt-8 group-hover:scale-110 transition"></div>
            
            <div className="flex items-start justify-between relative z-10">
              <div className="space-y-2">
                <p className="text-slate-500 text-sm font-medium">Total de Gastos</p>
                <p className="text-3xl font-bold text-slate-900">R$ 0,00</p>
                <p className="text-xs text-slate-400 mt-2">+0% este mês</p>
              </div>
              <div className="w-14 h-14 bg-gradient-to-br from-primary-100 to-primary-50 rounded-xl flex items-center justify-center group-hover:scale-110 transition">
                <svg className="w-7 h-7 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>

          {/* Categorias Card */}
          <div className="group relative bg-white rounded-2xl border border-slate-200 p-6 hover:border-success-300 transition shadow-sm hover:shadow-xl hover:shadow-success-500/10">
            <div className="absolute top-0 right-0 w-20 h-20 bg-success-50 rounded-full -mr-8 -mt-8 group-hover:scale-110 transition"></div>
            
            <div className="flex items-start justify-between relative z-10">
              <div className="space-y-2">
                <p className="text-slate-500 text-sm font-medium">Categorias</p>
                <p className="text-3xl font-bold text-slate-900">0</p>
                <p className="text-xs text-slate-400 mt-2">Sem limite</p>
              </div>
              <div className="w-14 h-14 bg-gradient-to-br from-success-100 to-success-50 rounded-xl flex items-center justify-center group-hover:scale-110 transition">
                <svg className="w-7 h-7 text-success-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                </svg>
              </div>
            </div>
          </div>

          {/* Últimos 30 dias Card */}
          <div className="group relative bg-white rounded-2xl border border-slate-200 p-6 hover:border-warning-300 transition shadow-sm hover:shadow-xl hover:shadow-warning-500/10">
            <div className="absolute top-0 right-0 w-20 h-20 bg-warning-50 rounded-full -mr-8 -mt-8 group-hover:scale-110 transition"></div>
            
            <div className="flex items-start justify-between relative z-10">
              <div className="space-y-2">
                <p className="text-slate-500 text-sm font-medium">Últimos 30 dias</p>
                <p className="text-3xl font-bold text-slate-900">0</p>
                <p className="text-xs text-slate-400 mt-2">Transações</p>
              </div>
              <div className="w-14 h-14 bg-gradient-to-br from-warning-100 to-warning-50 rounded-xl flex items-center justify-center group-hover:scale-110 transition">
                <svg className="w-7 h-7 text-warning-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        {/* Activity Section */}
        <div className="bg-white rounded-2xl border border-slate-200 p-8 shadow-sm">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h3 className="text-xl font-bold text-slate-900">Atividade Recente</h3>
              <p className="text-slate-500 text-sm">Seus gastos e transações</p>
            </div>
            <button className="px-4 py-2 rounded-lg bg-primary-50 text-primary-600 text-sm font-medium hover:bg-primary-100 transition">
              Ver Tudo
            </button>
          </div>
          
          <div className="text-center py-16">
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-slate-100 mb-4">
              <svg className="w-8 h-8 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <p className="text-slate-700 font-medium mb-1">Nenhuma atividade recente</p>
            <p className="text-slate-500 text-sm">Os gastos registrados aparecerão aqui</p>
          </div>
        </div>

        {/* Info Banner */}
        <div className="bg-gradient-to-r from-blue-50 to-blue-100 rounded-2xl border border-blue-200 p-6">
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <div className="inline-flex items-center justify-center w-10 h-10 rounded-lg bg-blue-600">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">Bem-vindo ao Zivro!</h4>
              <p className="text-blue-800 text-sm">
                Este é seu dashboard pessoal. Comece adicionando suas categorias e rastreie seus gastos para ter melhor controle financeiro. Acesse a documentação para mais informações.
              </p>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};
