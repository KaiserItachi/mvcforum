﻿namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Events;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Enums;

    public partial class VoteService : IVoteService
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        public VoteService(IMvcForumContext context, IMembershipUserPointsService membershipUserPointsService, ICacheService cacheService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _membershipUserPointsService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public Vote Get(Guid id)
        {
            return _context.Vote.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Vote vote)
        {
            // Delete any points associated with this vote
            _membershipUserPointsService.Delete(PointsFor.Vote, vote.Id);

            // Delete the vote
            _context.Vote.Remove(vote);
        }

        public IList<Vote> GetAllVotesByUser(Guid membershipId)
        {
            return _context.Vote.Where(x => x.VotedByMembershipUser.Id == membershipId).ToList();
        }

        public List<Vote> GetVotesByPosts(List<Guid> postIds)
        {
            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "GetVotesByPosts-", postIds.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Vote
                            .Include(x => x.VotedByMembershipUser)
                            .Include(x => x.User)
                            .Include(x => x.Post)
                            .AsNoTracking()
                            .Where(x => postIds.Contains(x.Post.Id)).ToList();
            });
        }

        public List<Vote> GetVotesByPost(Guid postId)
        {
            return _context.Vote
            .Include(x => x.VotedByMembershipUser)
            .Include(x => x.User)
            .Include(x => x.Post)
            .Where(x => x.Post.Id == postId).ToList();
        }

        /// <summary>
        /// Add a new vote
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        public Vote Add(Vote vote)
        {

            var e = new VoteEventArgs {Vote = vote};
            EventManager.Instance.FireBeforeVoteMade(this, e);

            if (!e.Cancel)
            {
                _context.Vote.Add(vote);

                EventManager.Instance.FireAfterVoteMade(this, new VoteEventArgs {Vote = vote});
            }

            return vote;
        }



    }
}
