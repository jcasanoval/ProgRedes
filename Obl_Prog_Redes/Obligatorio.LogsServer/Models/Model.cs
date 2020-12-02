﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obligatorio.LogsServer.Models
{
    public abstract class Model<E, M>
       where E : class
       where M : Model<E, M>, new()
    {
        public abstract E ToEntity();
        protected abstract M SetModel(E entity);

        public static IEnumerable<M> ToModel(IEnumerable<E> entities)
        {
            return entities.Select(x => ToModel(x));
        }

        public static M ToModel(E entity)
        {
            if (entity == null) return null;
            return new M().SetModel(entity);
        }

        public static IEnumerable<E> ToEntity(IEnumerable<M> models)
        {
            return models.Select(x => ToEntity(x));
        }

        public static E ToEntity(M model)
        {
            if (model == null) return null;
            return model.ToEntity();
        }
    }
}
