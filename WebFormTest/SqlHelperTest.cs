using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilityTool;
namespace WebFormTest
{
    [TestClass]
    public class SqlHelperTest
    {
        class TestObj {
            public string col1 { get; set; }
            public string col2 { get; set; }
            public string col3 { get; set; }
        }
        DataTable GenerateTestDt(bool withMap) {
            DataTable dt = new DataTable();
            if (withMap)
            {
                dt.Columns.Add(new DataColumn("col4", typeof(string)));
                dt.Columns.Add(new DataColumn("col5", typeof(string)));
                dt.Columns.Add(new DataColumn("col6", typeof(string)));
            }
            else {
                dt.Columns.Add(new DataColumn("col1", typeof(string)));
                dt.Columns.Add(new DataColumn("col2", typeof(string)));
                dt.Columns.Add(new DataColumn("col3", typeof(string)));
            }
            dt.Rows.Add(new object[] { "1", "2", "3" });
            dt.Rows.Add(new object[] { "4", "5", "6" });
            dt.Rows.Add(new object[] { "7", "8", "9" });
            return dt;
        }
        [TestMethod]
        public void TestToList()
        {
            var dt = GenerateTestDt(false);
            var expect = new List<TestObj>();
            foreach (DataRow dr in dt.Rows)
            {
                expect.Add(new TestObj {
                    col1 = Convert.ToString(dr["col1"]),
                    col2 = Convert.ToString(dr["col2"]),
                    col3 = Convert.ToString(dr["col3"]),
                });
            }
            var actual = dt.ToList<TestObj>();
            Assert.AreEqual(expect[0].col1, actual[0].col1);
        }
        [TestMethod]
        public void TestToListWithMap()
        {
            var dt = GenerateTestDt(true);

            var expect = new List<TestObj>();
            foreach (DataRow dr in dt.Rows)
            {
                expect.Add(new TestObj
                {
                    col1 = Convert.ToString(dr["col4"]),
                    col2 = Convert.ToString(dr["col5"]),
                    col3 = Convert.ToString(dr["col6"]),
                });
            }
            var map = new DataTableExtensions.TableColMap[] {
                new DataTableExtensions.TableColMap {  propertyName="col1",tableColumnName="col4"},
                new DataTableExtensions.TableColMap {  propertyName="col2",tableColumnName="col5"},
                new DataTableExtensions.TableColMap {  propertyName="col3",tableColumnName="col6"}
            };
            var actual = dt.ToList<TestObj>(map);
            Assert.AreEqual(expect[0].col1, actual[0].col1);
        }
        [TestMethod]
        public void ToObject()
        {
            var dt = GenerateTestDt(false);
            var dr = dt.Rows[0];
            var expect = new TestObj
                {
                    col1 = Convert.ToString(dr["col1"]),
                    col2 = Convert.ToString(dr["col2"]),
                    col3 = Convert.ToString(dr["col3"]),
                };
            var actual = dt.Rows[0].ToObject<TestObj>();
            Assert.AreEqual(expect.col1, actual.col1);
        }
        [TestMethod]
        public void ToObjectWithMap()
        {
            var dt = GenerateTestDt(true);
            var dr = dt.Rows[0];
            var expect = new TestObj
            {
                col1 = Convert.ToString(dr["col4"]),
                col2 = Convert.ToString(dr["col5"]),
                col3 = Convert.ToString(dr["col6"]),
            };
            var map = new DataTableExtensions.TableColMap[] {
                new DataTableExtensions.TableColMap {  propertyName="col1",tableColumnName="col4"},
                new DataTableExtensions.TableColMap {  propertyName="col2",tableColumnName="col5"},
                new DataTableExtensions.TableColMap {  propertyName="col3",tableColumnName="col6"}
            };
            var actual = dt.Rows[0].ToObject<TestObj>(map);
            Assert.AreEqual(expect.col1, actual.col1);
        }
    }
}
