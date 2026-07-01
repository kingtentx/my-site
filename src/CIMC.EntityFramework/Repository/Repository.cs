using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CIMC.EntityFramework
{
    /// <summary>
    /// 泛型数据访问层：支持任意DbContext
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <typeparam name="TContext">DbContext类型</typeparam>
    public class Repository<T, TContext> : IRepository<T>
        where T : class, new()
        where TContext : DbContext
    {
        protected readonly TContext _dbContext;
        private readonly Expression<Func<T, bool>> _autoDeleteCondition;

        /// <summary>
        /// 构造函数注入泛型DbContext
        /// </summary>
        public Repository(TContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _autoDeleteCondition = CreateAutoDeleteCondition();
        }

        #region 私有方法
        private Expression<Func<T, bool>> CreateAutoDeleteCondition()
        {
            var property = typeof(T).GetProperty("IsDelete");
            if (property != null && property.PropertyType == typeof(bool))
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var propertyAccess = Expression.Property(parameter, property);
                var falseValue = Expression.Constant(false);
                var equalsExp = Expression.Equal(propertyAccess, falseValue);
                return Expression.Lambda<Func<T, bool>>(equalsExp, parameter);
            }
            return null;
        }

        private IQueryable<T> GetBaseQuery()
        {
            var query = _dbContext.Set<T>().AsQueryable();
            if (_autoDeleteCondition != null)
                query = query.Where(_autoDeleteCondition);
            return query;
        }

        /// <summary>
        /// 尝试软删除（设置IsDelete = true）
        /// </summary>
        private bool TrySoftDelete(T entity)
        {
            var prop = typeof(T).GetProperty("IsDelete");
            if (prop == null || prop.PropertyType != typeof(bool)) return false;
            prop.SetValue(entity, true);
            return true;
        }

        #endregion

        #region 添加

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            _dbContext.SaveChanges();
            return entity;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool AddMany(IEnumerable<T> list)
        {
            _dbContext.Set<T>().AddRange(list);
            return _dbContext.SaveChanges() > 0;
        }
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<bool> AddManyAsync(IEnumerable<T> list)
        {
            await _dbContext.Set<T>().AddRangeAsync(list);
            return await _dbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return _dbContext.SaveChanges() > 0;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool UpdateMany(IEnumerable<T> list)
        {
            _dbContext.Set<T>().UpdateRange(list);
            return _dbContext.SaveChanges() > 0;
        }
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<bool> UpdateManyAsync(IEnumerable<T> list)
        {
            _dbContext.Set<T>().UpdateRange(list);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        #endregion

        #region 删除
        /// <summary>
        /// 删除（自动处理IsDelete软删除字段或物理删除）
        /// </summary>
        public bool Delete<TKey>(TKey id)
        {
            var entity = _dbContext.Set<T>().Find(id);
            if (entity == null) return false;
            // 优先尝试软删除
            if (TrySoftDelete(entity))
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _dbContext.Remove(entity); // 无IsDelete字段时物理删除
            }
            return _dbContext.SaveChanges() > 0;
        }
        /// <summary>
        /// 删除（自动处理IsDelete软删除字段或物理删除）
        /// </summary>
        public async Task<bool> DeleteAsync<TKey>(TKey id)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity == null) return false;
            // 优先尝试软删除
            if (TrySoftDelete(entity))
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _dbContext.Remove(entity); // 无IsDelete字段时物理删除
            }
            return await _dbContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        public bool DeleteMany(Expression<Func<T, bool>> where)
        {
            var query = GetBaseQuery().Where(where);

            if (_autoDeleteCondition != null)
            {
                var list = query.ToList();
                list.ForEach(e => TrySoftDelete(e));
                _dbContext.UpdateRange(list);
            }
            else
            {
                _dbContext.RemoveRange(query);
            }
            return _dbContext.SaveChanges() > 0;
        }

        /// <summary>
        /// 删除方法优化
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<bool> DeleteManyAsync(Expression<Func<T, bool>> where)
        {
            var query = GetBaseQuery().Where(where);

            if (_autoDeleteCondition != null)
            {
                var list = await query.ToListAsync();
                list.ForEach(e => TrySoftDelete(e));
                _dbContext.UpdateRange(list);
            }
            else
            {
                _dbContext.RemoveRange(query);
            }
            return await _dbContext.SaveChangesAsync() > 0;
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetOne<TKey>(TKey id)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey();
            if (primaryKey?.Properties.Count != 1)
                throw new InvalidOperationException("Only single primary key supported");

            var pkProperty = primaryKey.Properties[0].PropertyInfo;
            var parameter = Expression.Parameter(typeof(T));
            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, pkProperty),
                    Expression.Constant(id)),
                parameter);

            return GetBaseQuery().FirstOrDefault(predicate);
        }

        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> GetOneAsync<TKey>(TKey id)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey();
            if (primaryKey?.Properties.Count != 1)
                throw new InvalidOperationException("Only single primary key supported");

            var pkProperty = primaryKey.Properties[0].PropertyInfo;
            var parameter = Expression.Parameter(typeof(T));
            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, pkProperty),
                    Expression.Constant(id)),
                parameter);

            return await GetBaseQuery().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 条件获取单个实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public T GetOne(Expression<Func<T, bool>> where)
        {
            var model = GetBaseQuery().Where(where);
            return model?.FirstOrDefault();
        }
        /// <summary>
        /// 条件获取单个实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<T> GetOneAsync(Expression<Func<T, bool>> where)
        {
            var model = GetBaseQuery().Where(where);
            return await model?.FirstOrDefaultAsync();
        }

        /// <summary>
        /// 条件获取总条数
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> where)
            => GetBaseQuery().Where(where).Count();

        /// <summary>
        /// 条件获取总条数
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
            => await GetBaseQuery().Where(where).CountAsync();

        /// <summary>
        ///  条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> where)
        {
            return GetBaseQuery().Where(where);
        }

        /// <summary>
        ///  条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IEnumerable<T> GetModel(Expression<Func<T, bool>> where)
        {
            return GetBaseQuery().Where(where);
        }

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
        public (IQueryable<T> Queryable, int Count) GetQueryable<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = query.Count();
            var orderedQuery = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            var pagedQuery = orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return (pagedQuery, count);
        }

        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> where, string include)
        {
            return GetBaseQuery().Where(where).Include(include);
        }
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public IQueryable<T> GetQueryable<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include)
        {
            return GetBaseQuery().Where(where).Include(include);
        }
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public IEnumerable<T> GetModel(Expression<Func<T, bool>> where, string include)
        {
            return GetBaseQuery().Where(where).Include(include);
        }
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public IEnumerable<T> GetModel<TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include)
        {
            return GetBaseQuery().Where(where).Include(include);
        }
        /// <summary>
        /// 条件获取实体
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc">true:升序 , false:倒序 </param>
        /// <returns></returns>
        public IQueryable<T> GetQueryable<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            return isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public List<T> GetList()
        {
            return GetBaseQuery().ToList();
        }
        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync()
        {
            return await GetBaseQuery().ToListAsync();
        }

        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public List<T> GetList(Expression<Func<T, bool>> where)
        {
            return GetBaseQuery().Where(where).ToList();
        }
        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where)
        {
            return await GetBaseQuery().Where(where).ToListAsync();
        }

        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public List<T> GetList<TKey>(Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().AsQueryable();
            return isAsc ? query.OrderBy(orderBy).ToList() : query.OrderByDescending(orderBy).ToList();
        }
        /// <summary>
        ///  获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync<TKey>(Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().AsQueryable();
            return isAsc ? await query.OrderBy(orderBy).ToListAsync() : await query.OrderByDescending(orderBy).ToListAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<T> GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            return isAsc ? query.OrderBy(orderBy).ToList() : query.OrderByDescending(orderBy).ToList();
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            return isAsc ? await query.OrderBy(orderBy).ToListAsync() : await query.OrderByDescending(orderBy).ToListAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="topNum"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<T> GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int topNum, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            return isAsc ? query.OrderBy(orderBy).Take(topNum).ToList() : query.OrderByDescending(orderBy).Take(topNum).ToList();
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="topNum"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int topNum, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            return isAsc ? await query.OrderBy(orderBy).Take(topNum).ToListAsync() : await query.OrderByDescending(orderBy).Take(topNum).ToListAsync();
        }

        /// <summary>
        /// 获取列表 外键关联
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<T> GetList<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where).Include(include);
            return isAsc ? query.OrderBy(orderBy).ToList() : query.OrderByDescending(orderBy).ToList();
        }
        /// <summary>
        /// 获取列表 外键关联
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where).Include(include);
            return isAsc ? await query.OrderBy(orderBy).ToListAsync() : await query.OrderByDescending(orderBy).ToListAsync();
        }

        /// <summary>
        /// 获取列表 外键关联
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<T> GetList<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where).Include(include);
            return isAsc ? query.OrderBy(orderBy).ToList() : query.OrderByDescending(orderBy).ToList();
        }

        /// <summary>
        /// 获取列表 外键关联
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="where"></param>
        /// <param name="include"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> GetListAsync<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where).Include(include);
            return isAsc ? await query.OrderBy(orderBy).ToListAsync() : await query.OrderByDescending(orderBy).ToListAsync();
        }

        /// <summary>
        /// 分页获取列表
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public (List<T> List, int Count) GetList(Expression<Func<T, bool>> where, int pageIndex, int pageSize)
        {
            var query = GetBaseQuery().Where(where);
            var count = query.Count();
            var list = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return (list, count);
        }

        /// <summary>
        /// 分页获取列表
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(List<T> List, int Count)> GetListAsync(Expression<Func<T, bool>> where, int pageIndex, int pageSize)
        {
            var query = GetBaseQuery().Where(where);
            var count = await query.CountAsync();
            var list = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, count);
        }

        /// <summary>
        /// 分页获取列表 [有排序]
        /// </summary>
        /// <typeparam name="TKey">排序列</typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>      
        /// <param name="isAsc">true:升序 , false:倒序</param>
        /// <returns></returns>
        public (List<T> List, int Count) GetList<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = query.Count();
            var orderedQuery = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            var list = orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return (list, count);
        }
        /// <summary>
        ///  分页获取列表 [有排序]
        /// </summary>
        /// <typeparam name="TKey">排序列</typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isAsc">true:升序 , false:倒序</param>
        /// <returns></returns>
        public async Task<(List<T> List, int Count)> GetListAsync<TKey>(Expression<Func<T, bool>> where, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = await query.CountAsync();
            var orderedQuery = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            var list = await orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, count);
        }

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
        public (List<T> List, int Count) GetList<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = query.Count();
            var orderedQuery = isAsc ? query.Include(include).OrderBy(orderBy) : query.Include(include).OrderByDescending(orderBy);
            var list = orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return (list, count);
        }
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
        public async Task<(List<T> List, int Count)> GetListAsync<TKey>(Expression<Func<T, bool>> where, string include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = await query.CountAsync();
            var orderedQuery = isAsc ? query.Include(include).OrderBy(orderBy) : query.Include(include).OrderByDescending(orderBy);
            var list = await orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, count);
        }

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
        public (List<T> List, int Count) GetList<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = query.Count();
            var orderedQuery = isAsc ? query.Include(include).OrderBy(orderBy) : query.Include(include).OrderByDescending(orderBy);
            var list = orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return (list, count);
        }
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
        public async Task<(List<T> List, int Count)> GetListAsync<TKey, TProperty>(Expression<Func<T, bool>> where, Expression<Func<T, TProperty>> include, Expression<Func<T, TKey>> orderBy, int pageIndex, int pageSize, bool isAsc = false)
        {
            var query = GetBaseQuery().Where(where);
            var count = await query.CountAsync();
            var orderedQuery = isAsc ? query.Include(include).OrderBy(orderBy) : query.Include(include).OrderByDescending(orderBy);
            var list = await orderedQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, count);
        }
        #endregion

        #region 自定义SQL
        /// <summary>
        /// 执行自定义SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSql(string sql, params object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }
        /// <summary>
        /// 执行自定义SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        #endregion

        /// <summary>
        ///  添加事务
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public async Task<bool> TransactionalUpdateAsync(Func<Task> operation)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }

}
