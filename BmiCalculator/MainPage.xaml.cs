namespace BmiCalculator
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void btnCalculate_Clicked(object sender, EventArgs e)
        {
            try
            {
                double height = (Convert.ToDouble(HeightEntry.Text)) / 100;
                double weight = Convert.ToDouble(WeightEntry.Text);
                double bmi = weight / (height * height);

                lblAnswer.Text = "Your BMI is: " + bmi.ToString("F2");
            }
            catch (Exception)
            {
                lblAnswer.Text = "Please enter valid numbers for height and weight";
            }
        }
    }
}