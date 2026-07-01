using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CIMC.EntityFramework
{
    public interface IRepository<T> where T : class, new()
    {
        #region 添加
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        T Add(T entity);
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool AddMany(IEnumerable<T> list);
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<bool> AddManyAsync(IEnumerable<T> list);

        #endregion

        #region 更新
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Update(T entity);
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool UpdateMany(IEnumerable<T> list);
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<bool> UpdateManyAsync(IEnumerable<T> list);

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Delete<TKey>(TKey id);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync<TKey>(TKey id);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        bool DeleteMany(Expression<Func<T, bool>> where);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        Task<bool> DeleteManyAsync(Expression<Func<T, bool>> where);

        #endregion

        #region  获取数据        

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetOne<TKey>(TKey id);
        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> GetOneAsync<TKey>(TKey id);

        /// <summary>
        /// 条件获取单个实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        T GetOne(Expression<Func<T, bool>> where);
        /// <summary>
        /// 条件获取单个实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        Task<T> GetOneAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 条件获取总条数
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        int GetCount(Expression<Func<T, bool>> where);
        /// <summary>
        /// 条件获取总条数
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        Task<int> GetCountAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQueryable<T> GetQueryable(Expression<Func<T, bool>> where);
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IEnumerable<T> GetModel(Expression<Func<T, bool>> where);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        (IQueryable<T> Queryable, int Count) GetQueryable<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);

        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        IQueryable<T> GetQueryable(Expression<Func<T, bool>> where, string include);
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        IQueryable<T> GetQueryable<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include);
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        IEnumerable<T> GetModel(Expression<Func<T, bool>> where, string include);
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        IEnumerable<T> GetModel<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include);
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc">true:升序 , false:倒序 </param>
        /// <returns></returns>
        IQueryable<T> GetQueryable<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<T> GetList();
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetListAsync();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<T> GetList(Expression<Func<T, bool>> where);
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<T> GetList<TKey>(Expression<Func<T, TKey>> orderBy, bool isAsc = false);
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetListAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool isAsc = false);

        /// <summary>
        ///  获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        List<T> GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false);
        /// <summary>
        ///  获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false);

        /// <summary>
        ///  获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="topNum"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        List<T> GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int topNum, bool isAsc = false);
        /// <summary>
        ///  获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="topNum"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int topNum, bool isAsc = false);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        List<T> GetList<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, bool isAsc = false);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, bool isAsc = false);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        List<T> GetList<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, bool isAsc = false);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<List<T>> GetListAsync<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, bool isAsc = false);
        /// <summary>
        /// 分页获取列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        (List<T> List, int Count) GetList(Expression<Func<T, bool>> where, int pageIndex, int pageSize);
        /// <summary>
        /// 分页获取列表
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(List<T> List, int Count)> GetListAsync(Expression<Func<T, bool>> where, int pageIndex, int pageSize);

        /// <summary>
        /// 分页获取列表 [有排序]
        /// </summary>
        /// <typeparam name="K">排序列</typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="isAsc">true:升序 , false:倒序 </param>
        /// <returns></returns>
        (List<T> List, int Count) GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);
        /// <summary>
        /// 分页获取列表 [有排序]
        /// </summary>
        /// <typeparam name="K">排序列</typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="isAsc">true:升序 , false:倒序 </param>
        /// <returns></returns>
        Task<(List<T> List, int Count)> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);

        /// <summary>
        ///  分页获取列表 外键关联 [有排序]
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        (List<T> List, int Count) GetList<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);
        /// <summary>
        /// 分页获取列表 外键关联 [有排序]
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<(List<T> List, int Count)> GetListAsync<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);

        /// <summary>
        /// 分页获取列表 外键关联 [有排序]
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        (List<T> List, int Count) GetList<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);
        /// <summary>
        /// 分页获取列表 外键关联 [有排序]
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<(List<T> List, int Count)> GetListAsync<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false);
        #endregion

        #region 自定义SQL
        /// <summary>
        /// 执行自定义SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteSql(string sql, params object[] parameters);
        /// <summary>
        /// 执行自定义SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);

        #endregion

        Task<bool> TransactionalUpdateAsync(Func<Task> operation);


    }
}
