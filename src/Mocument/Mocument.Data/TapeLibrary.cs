using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mocument.Data
{
    public class TapeLibrary
    {
        private  TapesEntities _context;
        public TapeLibrary()
        {
            _context=new TapesEntities();
            _context.Connection.Open();
        }
        public void ResetContext()
        {
            _context.Dispose();
            _context = new TapesEntities();
            _context.Connection.Open();
        }
        static readonly Func<TapesEntities, string, Tape> SelectById =
            CompiledQuery.Compile<TapesEntities, string, Tape>((ctx, id) =>
                (from e in ctx.Tapes where e.Id == id select e).FirstOrDefault());
        
        public int Count
        {
            get
            {
                return _context.Tapes.Count();
            }
        }

        public void Delete(string tapeId)
        {
            
            Tape tape = _context.Tapes.FirstOrDefault(t => t.Id == tapeId);
            _context.Tapes.DeleteObject(tape);
            _context.SaveChanges();
        }

        public void AddOrUpdate(Tape tape)
        {
            
            string id = tape.Id;
            if (_context.Tapes.Any(e => e.Id == id))
            {
                _context.Tapes.Attach(tape);
                _context.ObjectStateManager.ChangeObjectState(tape, EntityState.Modified);
            }
            else
            {
                _context.Tapes.AddObject(tape);
            }


            _context.SaveChanges();
        }

        public List<Tape> Select()
        {
            using (var context = new TapesEntities())
            {
                // context.Tapes.MergeOption = MergeOption.NoTracking;
                return context.Tapes.ToList();
            }
        }

        public Tape Select(string tapeId)
        {
            
            return SelectById.Invoke(_context, tapeId);
        }

        public List<Tape> SelectMatches(string pattern)
        {
            
            return _context.Tapes.Where(t => Regex.IsMatch(t.Id, pattern, RegexOptions.IgnoreCase)).ToList();
        }
    }
}