using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{monikar}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> GetTalks(string monikar)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(monikar);

                return _mapper.Map<TalkModel[]>(talks);

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR TYPE : {ex.GetType()}");
            }
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalkById(string monikar, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(monikar, id);
                if (talk == null) return NotFound();

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR TYPE : {ex.GetType()}");
            }
        }

          
        [HttpPost]
        public async Task<ActionResult<TalkModel>> PostTalk(string monikar ,TalkModel talkModel)
        {
            try
            {
                var camp = await _repository.GetCampAsync(monikar);
                if (camp == null) return NotFound("camp not found");

                var talk = await _repository.GetTalkByMonikerAsync(monikar, talkModel.TalkId);
                if (talk != null) return BadRequest("talk in use");

                var location = _linkGenerator.GetPathByAction("GetTalkById", "Talks", new { monikar = monikar, id = talkModel.TalkId });
                if (string.IsNullOrWhiteSpace(location)) return BadRequest("unable to craete link");

                talk = _mapper.Map<Talk>(talkModel);
                _repository.Add(talk);
                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR TYPE : {ex.GetType()}");
            }

            return BadRequest();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTalk(string monikar, int id)
        {
            try
            {
                var item = await _repository.GetTalkByMonikerAsync(monikar, id);
                if (item == null) return NotFound();
                _repository.Delete(item);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete talk");
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR TYPE : {ex.GetType()}");
            }
        }
    }
}
