﻿using Akka.Actor;
using Akka.Event;
using Akka.Cassandra.Cqrs.Core;
using Akka.Cassandra.Cqrs.Core.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Cassandra.Cqrs.Core.Domain;

namespace Actors
{
    public class AccountActor : AggregateRootActor<AccountActor>, IPersistable
    {
        private ILoggingAdapter _log;
        private AccountState _state;

        public AccountActor(IAggregateRootCreationParameters parameters)
            : base(parameters)
        {
            _log = Context.GetLogger();
            _state = new AccountState(parameters.Id, this);

            //_log.Info("Ctor AccountActor: {0}, {1}", this.PersistenceId, this.ToString());
        }

        protected override bool Handle(ICommand command)
        {
            _state.Handle(command);
            return true;
        }

        protected override bool Apply(IEvent @event)
        {
            _state.Mutate(@event);
            return true;
        }

        protected override bool RecoverState(object state, long version)
        {
            if (state.CanHandle<AccountState>(x =>
                {
                    x.Events = this;
                    _state = x;
                }))
            {
                return true;
            }

            return false;
        }

        protected override object GetState()
        {
            return _state;
        }

        public override string PersistenceId
        {
            get { return _state.PersistenceId; }
        }
    }
}
