using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        //[HttpGet("{moniker}")]
        //public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        //{
        //    try
        //    {
        //        var result = await _repository.GetCampAsync(moniker);
        //        if (result == null) return NotFound();

        //        CampModel camp = _mapper.Map<CampModel>(result);

        //        return camp;
        //    }
        //    catch (Exception)
        //    {
        //        return this.StatusCode(StatusCodes.Status500InternalServerError, "maybe db failure");
        //    }
        //}

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null)
                    return NotFound();

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "maybe db failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "maybe db failure");
            }
        }


        [HttpPost]
        public async Task<ActionResult<CampModel>> PostCamp(CampModel campModel)
        {
            try
            {
                var campMonikar = await _repository.GetCampAsync(campModel.Moniker);
                if (campMonikar != null)
                {
                    return BadRequest("Monikar in use");
                }

                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps", new { moniker = campModel.Moniker });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("monikar is no valid");
                }

                var camp = _mapper.Map<Camp>(campModel);
                _repository.Add(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Type of Error: {ex.GetType().ToString()} \n Error Message : {ex.Message}");
            }

            return BadRequest();
        }



        [HttpPut("{monikar}")]
        public async Task<ActionResult<CampModel>> PutCamp(string monikar, CampModel campModel)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(monikar);
                if (oldCamp == null) return NotFound();

                _mapper.Map(campModel, oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"ERROR TYPE : {ex.GetType()}");
            }

            return BadRequest();
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteCamp(string monikar)
        {
            try
            {
                var ToBeDeletedCamp = await _repository.GetCampAsync(monikar);
                if (ToBeDeletedCamp == null) return NotFound();

                _repository.Delete(ToBeDeletedCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"ERROR TYPE : {ex.GetType()}");
            }

            return BadRequest();
        }

    }
}
