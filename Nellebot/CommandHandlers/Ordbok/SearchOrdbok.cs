﻿using DSharpPlus.CommandsNext;
using MediatR;
using Nellebot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nellebot.CommandHandlers.Ordbok
{
    public class SearchOrdbok
    {
        public class SearchOrdbokRequest : CommandRequest
        {
            public string Query { get; set; } = string.Empty;
            public string Dictionary { get; set; } = string.Empty;

            public SearchOrdbokRequest(CommandContext ctx) : base(ctx)
            {
            }
        }

        public class SearchOrdbokHandler : AsyncRequestHandler<SearchOrdbokRequest>
        {
            private readonly OrdbokHttpClient _ordbokClient;

            public SearchOrdbokHandler(OrdbokHttpClient ordbokClient)
            {
                _ordbokClient = ordbokClient;
            }

            protected override async Task Handle(SearchOrdbokRequest request, CancellationToken cancellationToken)
            {
                var ctx = request.Ctx;

                var searchResponse = await _ordbokClient.Search(request.Dictionary, request.Query);

                if (searchResponse == null)
                {
                    await ctx.RespondAsync($"no result");
                }
                else
                {
                    var match = searchResponse.FirstOrDefault(x => x.Lemmas.Any(l => l.Value == request.Query));

                    if(match == null)
                    {
                        match = searchResponse.FirstOrDefault();
                    }

                    var responseAsString = JsonSerializer.Serialize(match);

                    await ctx.RespondAsync($"`{responseAsString.Substring(0, 1000)}`");
                }
            }
        }
    }
}
